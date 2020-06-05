// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Injectable } from '@angular/core';
import {
    CanActivate,
    ActivatedRouteSnapshot,
    RouterStateSnapshot
} from '@angular/router';
import { AuthService } from '../services';
import { Observable } from 'rxjs';
import { tap, take } from 'rxjs/operators';

@Injectable()
export class PromptLoginForUnauthGuard implements CanActivate {

    constructor(private authService: AuthService) {
    }

    canActivate(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
    ): Observable<boolean> {
        return this.authService
            .getAuthenticationStatus()
            .pipe(
                take(1),
                tap((isAuthenticated) => {
                    if (!isAuthenticated) {
                        this.authService.navigateToLogin(state.url);
                    }
                })
            );
    }
}
