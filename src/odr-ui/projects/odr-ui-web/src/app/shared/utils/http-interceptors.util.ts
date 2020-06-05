// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


import { throwError as observableThrowError, Observable, pipe, combineLatest } from 'rxjs';
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable, Injector, NgZone } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { AppInsights } from 'applicationinsights-js';
import { appInsightsContextStream } from '../../../app-insights';
import { mergeMap, catchError, map } from 'rxjs/operators';

const observeOnZone = <T>(zone: NgZone) => (source: Observable<T>) => new Observable<T>((subscriber) => {
    source.subscribe({
        next: (value) => zone.run(() => subscriber.next(value)),
        error: (err) => zone.run(() => subscriber.error(err)),
        complete: () => zone.run(() => subscriber.complete()),
    });
});

@Injectable()
export class AuthorizedRequestInterceptor implements HttpInterceptor {

    constructor(private injector: Injector) {
    }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // console.log('intercept', req.method, req.urlWithParams);
        const zone: NgZone = this.injector.get(NgZone);
        const authService = this.injector.get(AuthService);
        const result =
            combineLatest(appInsightsContextStream(), authService.getBearerToken())
                .pipe(
                    mergeMap(([data, bearerToken]) => {
                        let headers = req.headers;
                        if (data.userId) {
                            headers = headers.set('ai_user', data.userId);
                        }
                        if (data.sessionId) {
                            headers = headers.set('ai_session', data.sessionId);
                        }
                        if (bearerToken) {
                            const authHeader = `Bearer ${bearerToken}`;
                            headers = headers.set('Authorization', authHeader);
                        }
                        const nextReq = req.clone({
                            headers
                        });
                        return next.handle(nextReq);
                    }),
                    observeOnZone(zone)
                );

        return result;
    }
}

// Fix for issue https://github.com/angular/angular/issues/19103
@Injectable()
export class JsonErrorResponseInterceptor implements HttpInterceptor {
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(
            catchError(event => {
                if (event instanceof HttpErrorResponse) {
                    const response = event as HttpErrorResponse;
                    if (/^application\/json/i.test(response.headers.get('content-type'))) {
                        return observableThrowError(new HttpErrorResponse({
                            error: response.error,
                            headers: response.headers,
                            status: response.status,
                            statusText: response.statusText,
                            url: response.url,
                        }));
                    }
                }
                return observableThrowError(event);
            })
        );
    }
}
