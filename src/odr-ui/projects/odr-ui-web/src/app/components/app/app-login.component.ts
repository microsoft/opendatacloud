import {Component, OnInit} from '@angular/core';
import {AuthService} from '../../shared/services';

@Component({
    selector: 'app-login',
    template: `<div></div>`
})
export class AppLoginComponent implements OnInit {

    constructor(private authService: AuthService) {
    }

    ngOnInit(): void {
      this.authService.redirectToLoginPage();
    }
}
