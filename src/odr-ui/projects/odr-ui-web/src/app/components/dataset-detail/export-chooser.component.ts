// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Dataset } from '../../shared/types/dataset.type';
import { AuthService } from '../../shared/services';

function buildEnableLink({ publisherId, offerId, planId }): any {
    return [
        'https://ms.portal.azure.com/',
        '#blade/Microsoft_Azure_Marketplace/LegalTermsSkuProgrammaticAccessBlade/legalTermsSkuProgrammaticAccessData/',
        encodeURIComponent(JSON.stringify({ product: { publisherId, offerId, planId } }))
    ].join('');
}
// To show deployment options on the UI under Deploy to Azure option
const deploymentTypes = [
    {
        deploymentId: 'create-ubuntu-dsvm',
        description: 'Data Science Virtual Machine (Ubuntu)',
        enableLink: buildEnableLink({
            publisherId: 'microsoft-ads',
            offerId: 'linux-data-science-vm-ubuntu',
            planId: 'linuxdsvmubuntu'
        })
    },
    {
        deploymentId: 'create-windows-dsvm',
        description: 'Data Science Virtual Machine (Windows)',
        enableLink: buildEnableLink({
            publisherId: 'microsoft-ads',
            offerId: 'windows-data-science-vm',
            planId: 'windows2016'
        })
    },
    {
        deploymentId: 'create-ubuntu-vm',
        description: 'Virtual Machine (Ubuntu)',
        enableLink: null
    },
    {
        deploymentId: 'create-windows-vm',
        description: 'Virtual Machine (Windows)',
        enableLink: null
    },
    {
        deploymentId: 'create-synapse-ws',
        description: 'Synapse Workspace',
        enableLink: null
     }
];

@Component({
    selector: 'app-export-chooser',
    templateUrl: './export-chooser.component.html',
    styleUrls: ['./export-chooser.component.scss']
})
export class ExportChooserComponent {

    @Input() dataset: Dataset;
    @Input() datasetDownloadUrl: string;
    @Input() isAuthenticated: boolean;
    @Input() acceptedLicense: boolean;
    @Input() isCompressedAvailable: boolean;
    @Output() onShowDownloadDialog: EventEmitter<boolean> = new EventEmitter();
    @Output() onDownloadZipFile: EventEmitter<boolean> = new EventEmitter();
    @Output() onDownloadGzipFile: EventEmitter<boolean> = new EventEmitter();
    @Output() onDeployDataset: EventEmitter<any> = new EventEmitter();
    @Output() openLicense: EventEmitter<any> = new EventEmitter();

    deploymentTypes = deploymentTypes;
    deploymentId: string = deploymentTypes[0].deploymentId;
    areDeployOptionsVisible = false;

    constructor(
        private authService: AuthService
    ) { }

    downloadZipFile() {
        this.onDownloadZipFile.emit(true);
    }

    downloadGzipFile() {
        this.onDownloadGzipFile.emit(true);
    }

    downloadDataset() {
        this.onShowDownloadDialog.emit(true);
    }

    canExport() {
        return this.isAuthenticated && this.acceptedLicense;
    }

    showLogin(): void {
        this.authService.navigateToLogin();
    }

    toggleDeployOptions() {
        this.areDeployOptionsVisible = !this.areDeployOptionsVisible;
    }

    closeOptions() {
        this.areDeployOptionsVisible = false;
    }

    deployDataset() {
        this.onDeployDataset.emit({ deploymentId: this.deploymentId, datasetId: this.dataset.id });
    }

    get enableLink() {
        const selected = deploymentTypes.find(({ deploymentId }) => deploymentId === this.deploymentId);
        return selected ? selected.enableLink : null;
    }
}
