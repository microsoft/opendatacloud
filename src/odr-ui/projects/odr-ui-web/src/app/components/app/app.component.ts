import { Component } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { combineLatest, Observable } from 'rxjs';
import { filter, startWith, map, tap, switchMap } from 'rxjs/operators';
import { AuthService, OdrService } from '../../shared/services';
import { AppInsights } from 'applicationinsights-js';
import { environment } from '../../../environments/environment';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss']
})
export class AppComponent {
    isMenuOpen = false;
    public isAuthenticated: Observable<boolean>;
    public canNominateDataset: Observable<boolean>;

    constructor(
        private authService: AuthService,
        private odrService: OdrService,
        private router: Router) {

        const routerStream = router.events
            .pipe(
                filter(event => event instanceof NavigationEnd)
            );

        const authStream = this.authService.getAuthenticationStatus()
            .pipe(
                startWith(false),
                map((isAuthenticated): string => {
                    if (!isAuthenticated) {
                        return null;
                    }

                    const { uniqueId } = this.authService.getAuthenticatedUserDetails();
                    return uniqueId || 'unknown';
                })
            );

        combineLatest(routerStream, authStream)
            .pipe(
                tap(([event, userId]: [NavigationEnd, string]) => {
                    window.scrollTo(0, 0);
                    if (environment.appInsights.enableAppInsights) {
                        if (userId) {
                            AppInsights.setAuthenticatedUserContext(userId);
                        } else {
                            AppInsights.clearAuthenticatedUserContext();
                        }
                        AppInsights.trackPageView(null, event.urlAfterRedirects);
                    }
                })
            )
            .subscribe();

        this.isAuthenticated = this.authService.getAuthenticationStatus();
        this.canNominateDataset = authStream.pipe(
          switchMap(() => odrService.getCurrentUserDetails()),
          map(cu => cu.canNominateDataset)
        );
    }

    public onLogin(): void {
        this.authService.navigateToLogin();
    }

    onResize(event) {
        if (event && event.target && event.target.innerWidth && event.target.innerWidth > 720) {
            this.isMenuOpen = false;
        }
    }

    public onLogout(): void {
        this.authService.logout();
    }

    public get userName() {
        return this.authService.getAuthenticatedUserDetails().name;
    }
}
