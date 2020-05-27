import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable, Injector } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Observable } from 'rxjs';
import { mergeMap } from 'rxjs/operators';

@Injectable()
export class AuthorizedRequestInterceptor implements HttpInterceptor {

    constructor(private injector: Injector) {
    }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const authService = this.injector.get(AuthService);
        return authService
            .getCurrentUser()
            .pipe(
                mergeMap(({ bearerToken }) => {
                    let headers = req.headers;
                    if (bearerToken) {
                        const authHeader = `Bearer ${bearerToken}`;
                        headers = headers.set('Authorization', authHeader);
                    }
                    const nextReq = req.clone({
                        headers
                    });
                    return next.handle(nextReq);
                })
            );
    }
}
