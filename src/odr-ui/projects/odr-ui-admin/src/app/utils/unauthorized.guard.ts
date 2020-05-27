import { Injectable } from '@angular/core';
import {
    CanActivate,
    ActivatedRouteSnapshot,
    RouterStateSnapshot
} from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Observable } from 'rxjs';
import { tap, map } from 'rxjs/operators';

@Injectable()
export class UnauthorizedGuard implements CanActivate {

    constructor(private authService: AuthService) {
    }

    canActivate(route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot): Observable<boolean> {
        return this.authService
            .getCurrentUser()
            .pipe(
                tap(({ isAuthenticated }) => {
                    // console.log(`isAuthenticated: ${isAuthenticated}`);
                    if (!isAuthenticated) {
                        this.authService.navigateToUnauthenticatedPage();
                    }
                }),
                map(({ isAuthenticated }) => isAuthenticated)
            );
    }
}
