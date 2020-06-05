// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { License } from 'odr-ui-shared';
import { AuthService } from '../../shared/services';

@Component({
    selector: 'app-license-acceptance',
    templateUrl: './license-acceptance.component.html',
    styleUrls: ['./license-acceptance.component.scss']
})
export class LicenseAcceptanceComponent {

    @Input() license: License;
    @Input() isAuthenticated: boolean;
    @Input() acceptedLicense: boolean;
    @Output() openLicense: EventEmitter<any> = new EventEmitter();

    constructor(private authService: AuthService) {
    }

    public onLogin(): void {
        this.authService.navigateToLogin();
    }

    openLicenseClicked() {
        this.openLicense.emit({});
    }
}
