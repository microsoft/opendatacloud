import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Observable } from 'rxjs';
import { CurrentUser } from '../models/current-user.model';

@Component({
    selector: 'app-nav-menu',
    templateUrl: './nav-menu.component.html',
    styleUrls: ['./nav-menu.component.scss']
})
export class NavMenuComponent {

    isExpanded = false;
    currentUser: Observable<CurrentUser>;

    constructor(private authService: AuthService) {
        this.currentUser = this.authService.getCurrentUser();
    }

    collapse() {
        this.isExpanded = false;
    }

    toggle() {
        this.isExpanded = !this.isExpanded;
    }

    onLogin() {
        this.authService.login();
    }

    onLogout() {
        this.authService.logout();
    }
}
