// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, EventEmitter, Input, Output, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { Dataset, DatasetDownloadDialogConfig, LicenseDialogConfig, FileEntry } from '../../shared/types';
import { Router } from '@angular/router';
import { License } from 'odr-ui-shared';
import { OdrService } from '../../shared/services/odr.service';
import { CreateDeployment } from '../../shared/types/create-deployment.type';
import { LicenseDialogComponent } from './license-dialog.component';
import { DatasetDownloadDialogComponent } from './dataset-download-dialog.component';
import { Subscription, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { AuthService } from '../../shared/services';
import { ExportActionType } from '../../shared/types/export-action-type.enum';
import { ModalDialogService } from 'odr-ui-shared';

interface OnUserAcceptedLicenseArgs {
    datasetId: string;
    reason: string;
}

@Component({
    selector: 'app-dataset-detail',
    styleUrls: ['./dataset-detail.component.scss'],
    templateUrl: './dataset-detail.component.html'
})
export class DatasetDetailComponent implements OnInit, OnDestroy {

    @Input() isAuthenticated: boolean;
    @Input() acceptedLicense: boolean;
    @Input() dataset: Dataset;
    @Output() onUserAcceptedLicense: EventEmitter<OnUserAcceptedLicenseArgs> = new EventEmitter();
    @ViewChild('downloadLink', {static: false}) private downloadLink: ElementRef;

    public get datasetId(): string { return this.dataset.id; }

    public get isCompressedAvailable(): boolean { return this.dataset.isCompressedAvailable; }

    datasetDownloadUrl = '';
    datasetFileDownloadUrl = '';
    zipDownloadUrl = '';
    gzipDownloadUrl = '';
    currentDownloadUrl = '';
    currentFileId = '';
    licenseSubscription: Subscription;
    license: License;
    ExportActionType = ExportActionType;
    currentAction: ExportActionType = ExportActionType.None;
    deployParams: any = null;

    constructor(
        private router: Router,
        private odrService: OdrService,
        private modal: ModalDialogService,
        private authService: AuthService
    ) {
    }

    public ngOnInit() {
        this.license = {
            name: this.dataset.licenseName,
            id: this.dataset.licenseId,
            contentUri: this.dataset.licenseContentUri
        };

        setTimeout(() => {
            this.licenseSubscription = this.odrService.getLicenseById(this.dataset.licenseId)
                .pipe(
                    tap(license => {
                        this.license = license;
                    })
                )
                .subscribe();
        }, 0);
    }

    public ngOnDestroy() {
        this.licenseSubscription.unsubscribe();
    }

    public openIssue(): void {
        this.router.navigate(['/issue', this.dataset.id]);
    }

    public userDidAcceptLicense({ reason }: { reason: string }) {
        this.onUserAcceptedLicense.emit({
            datasetId: this.dataset.id,
            reason
        });

        this.processAction();
    }

    public onShowDownloadDialog() {
        this.currentAction = ExportActionType.DownloadDataset;
        this.datasetDownloadUrl = '';

        if (this.validateExportAndContinue()) {
            this.showDownloadDialog();
        }
    }

    public onDownloadZipFile() {
        this.currentAction = ExportActionType.DownloadZip;
        this.zipDownloadUrl = '';

        if (this.validateExportAndContinue()) {
            this.downloadZipFile();
        }
    }

    public onDownloadGzipFile() {
        this.currentAction = ExportActionType.DownloadGzip;
        this.gzipDownloadUrl = '';

        if (this.validateExportAndContinue()) {
            this.downloadGzipFile();
        }
    }

    public onDeployDataset(params) {
        this.deployParams = params;
        this.currentAction = ExportActionType.Deploy;

        if (this.validateExportAndContinue()) {
            this.deployDataset();
        }
    }

    public onDownloadFile(file) {
        this.currentAction = ExportActionType.DownloadFile;
        this.currentFileId = file && file.id;

        if (this.validateExportAndContinue()) {
            this.downloadFile();
        }
    }

    public openLicense(): void {
        const config = new LicenseDialogConfig();
        config.license = this.license;
        config.isAuthenticated = this.isAuthenticated;
        config.acceptedLicense = this.acceptedLicense;

        this.modal.create({
          title: '',
          component: LicenseDialogComponent,
          params: {
            config
          },
          onClose: ({ userAccept, userReason }) => {
            if (userAccept) {
                this.userDidAcceptLicense({
                    reason: userReason
                });
            }
          }
        });

        // const modal = this.modalService.create({
        //     nzTitle: '',
        //     nzContent: LicenseDialogComponent,
        //     nzComponentParams: {
        //         config: config
        //     },
        //     nzFooter: null
        // });

        // // Return a result when closed
        // modal.afterClose.subscribe(({ userAccept, userReason }) => {
        //     if (userAccept) {
        //         this.userDidAcceptLicense({
        //             reason: userReason
        //         });
        //     }
        // });
    }

    private processAction() {
        switch (this.currentAction) {
            case ExportActionType.DownloadFile:
                this.downloadFile();
                break;
            case ExportActionType.DownloadZip:
                this.downloadZipFile();
                break;
            case ExportActionType.DownloadGzip:
                this.downloadGzipFile();
                break;
            case ExportActionType.DownloadDataset:
                this.showDownloadDialog();
                break;
            case ExportActionType.Deploy:
                this.deployDataset();
                break;
            default:
                // do nothing
                break;
        }

        this.currentAction = ExportActionType.None;
    }

    private validateExportAndContinue() {
        if (!this.isAuthenticated) {
            this.authService.navigateToLogin();
            return false;
        }

        if (!this.acceptedLicense) {
            this.openLicense();
            return false;
        }

        return true;
    }

    private deployDataset() {
        const deploymentId = this.deployParams && this.deployParams.deploymentId;
        const datasetId = this.deployParams && this.deployParams.datasetId;
        const payload: CreateDeployment = { datasetId, deploymentId };
        const importWindow = window.open('about:blank', '_blank');

        this.odrService
            .createDeployment(payload)
            .pipe(
                tap((deploymentUrl) => {
                    this.currentAction = ExportActionType.None;
                    importWindow.location.href = deploymentUrl;
                })
            )
            .subscribe({
                error(err) {
                    console.error(err);
                }
            });
    }

    private openDownloadDialog(url: string): void {
        const config = new DatasetDownloadDialogConfig();
        config.dataset = this.dataset;
        config.datasetUrl = url;

        this.modal.create({
          title: '',
          component: DatasetDownloadDialogComponent,
          params: {
            config
          }
        });

        // const modal = this.modalService.create({
        //     nzTitle: '',
        //     nzContent: DatasetDownloadDialogComponent,
        //     nzComponentParams: {
        //         config: config
        //     },
        //     nzFooter: null
        // });

        // Return a result when closed
        // modal.afterClose.subscribe(result => console.log('[afterClose] The result is:', result));
    }

    private startDownload(fileUrl: string): void {
        const link = this.downloadLink.nativeElement;
        link.href = fileUrl;
        link.click();
    }

    private downloadFile(): void {
        this.datasetFileDownloadUrl = '';
        this.currentDownloadUrl = '';
        this.odrService
            .getDatasetFileDownloadUrl(this.datasetId, this.currentFileId)
            .pipe(
                tap((result: string) => {
                    this.datasetFileDownloadUrl = result;
                    this.currentAction = ExportActionType.None;
                    this.startDownload(this.datasetFileDownloadUrl);
                    this.currentFileId = null;
                }),
                catchError(error => {
                    this.currentDownloadUrl = error.statusText;
                    return of(error);
                })
            )
            .subscribe();
    }

    private downloadZipFile() {
        this.odrService
            .getZipFileDownloadUrl(this.datasetId)
            .pipe(
                tap((result: string) => {
                    this.zipDownloadUrl = result;
                    this.currentAction = ExportActionType.None;
                    this.startDownload(this.zipDownloadUrl);
                }),
                catchError(error => {
                    return of(error);
                })
            )
            .subscribe();
    }

    private downloadGzipFile() {
        this.odrService
            .getGzipFileDownloadUrl(this.datasetId)
            .pipe(
                tap((result: string) => {
                    this.gzipDownloadUrl = result;
                    this.currentAction = ExportActionType.None;
                    this.startDownload(this.gzipDownloadUrl);
                }),
                catchError(error => {
                    return of(error);
                })
            )
            .subscribe();
    }

    private showDownloadDialog() {
        this.odrService
            .getDatasetDownloadUrl(this.datasetId)
            .pipe(
                tap((result: string) => {
                    this.datasetDownloadUrl = result;
                    this.currentDownloadUrl = this.datasetDownloadUrl;
                    this.currentAction = ExportActionType.None;
                    this.openDownloadDialog(this.datasetDownloadUrl);
                })
            )
            .subscribe();
    }
}
