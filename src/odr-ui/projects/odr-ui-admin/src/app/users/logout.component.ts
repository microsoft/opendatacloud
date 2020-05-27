import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import {Subscription, merge} from 'rxjs';
import {tap} from 'rxjs/operators';
import {ActivatedRoute, Router} from '@angular/router';

@Component({
    selector: 'app-signout',
    templateUrl: './logout.component.html',
    styleUrls: ['./logout.component.scss']
})
export class LogoutComponent implements OnInit, OnDestroy {

    subscription: Subscription;

    text = 'You have logged out of the application.';

    constructor(
        private authService: AuthService,
        private router: Router,
        private route: ActivatedRoute
    ) {
    }

    ngOnInit(): void {

        const currentUserStream = this.authService
            .getCurrentUser()
            .pipe(
                tap(({ isAuthenticated }) => {
                    if (isAuthenticated) {
                        this.router.navigate(['/']);
                    }
                })
            );

        const unauthorizedStream = this.route.paramMap.pipe(
            tap(params => {
                if (Boolean(params.get('unauthorized'))) {
                    this.text = 'UNAUTHORIZED';
                    setTimeout(() => {
                        this.authService.logout();
                    }, 3000);
                }
            })
        );

        this.subscription = merge(currentUserStream, unauthorizedStream).subscribe();
    }

    ngOnDestroy(): void {
        this.subscription.unsubscribe();
    }
}
