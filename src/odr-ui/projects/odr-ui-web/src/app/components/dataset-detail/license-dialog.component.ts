// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { OdrService } from '../../shared/services';
import { LicenseDialogConfig, LicenseDialogResult } from '../../shared/types';
import { License } from 'odr-ui-shared';
import { Subscription, of } from 'rxjs';
import { tap, mergeMap } from 'rxjs/operators';
import { AngularEditorConfig } from '@kolkov/angular-editor';
import { ModalDialogService } from 'odr-ui-shared';

@Component({
    selector: 'app-license-dialog',
    styleUrls: ['../../shared/styles/dialog.component.css', './license-dialog.component.scss'],
    templateUrl: './license-dialog.component.html'
})
export class LicenseDialogComponent implements OnInit, OnDestroy {
    @Input() config: LicenseDialogConfig;

    editorConfig: AngularEditorConfig = {
        editable: false,
        spellcheck: false,
        height: '500px',
        minHeight: '500px',
        width: 'auto',
        minWidth: '0',
        placeholder: '',
        translate: 'no',
        showToolbar: false,
        enableToolbar: false
      };

    isAuthenticated: boolean;
    acceptedLicense: boolean;
    datasetId: string;
    isBusy = false;
    license: License;
    subscription: Subscription;
    minimumUserReasonLength = 10;
    userReason: string;
    licenseContent: string;
    baseUrl: string;
    licenseFileUrl: string;

    constructor(
        private modal: ModalDialogService,
        private odrService: OdrService) {
    }

    ngOnInit() {
        if (!this.config) {
            console.log('no config');
            return;
        }

        this.license = this.config.license;
        this.isAuthenticated = this.config.isAuthenticated;
        this.acceptedLicense = this.config.acceptedLicense;
        this.userReason = this.config.userReason || '';
        this.baseUrl = this.odrService.getBaseUrl();

        if (!this.license.isFileBased) {
            this.subscription = of(null)
                .pipe(
                    tap(() => {
                        setTimeout(() => {
                            this.isBusy = true;
                        });
                    }),
                    mergeMap(() => {
                        return this.odrService.getLicenseContentByUrl(this.license.contentUri);
                    }),
                    tap((result: any) => {
                        setTimeout(() => {
                            this.licenseContent = result;
                            this.isBusy = false;
                        });
                    })
                )
                .subscribe();
        } else {
            this.licenseFileUrl = `${this.baseUrl}${this.license.contentUri}`;
        }
    }

    ngOnDestroy() {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }

    public get isValid() {
        return this.userReason.length >= this.minimumUserReasonLength;
    }

    // Before dismissing dialog do a check
    public beforeDismiss(): boolean {
        return true;
    }

    // Before close action is fired do a check.
    public beforeClose(): boolean {
        return false;
    }

    // Click of action button to close dialog
    public onClose(closeValue: boolean): void {
        const result: LicenseDialogResult = new LicenseDialogResult();
        result.userAccept = closeValue;
        result.userReason = this.userReason;
        this.modal.close(result);
    }
}
