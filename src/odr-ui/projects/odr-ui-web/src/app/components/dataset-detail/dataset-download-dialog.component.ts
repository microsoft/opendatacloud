import { Component, Input, OnInit } from '@angular/core';
import { DatasetDownloadDialogConfig, Dataset } from '../../shared/types';
import { ToasterService } from 'angular2-toaster';
import { ModalDialogService } from 'odr-ui-shared';

@Component({
    selector: 'app-dataset-download-dialog',
    styleUrls: ['../../shared/styles/dialog.component.css', './dataset-download-dialog.component.scss'],
    templateUrl: './dataset-download-dialog.component.html'
})
export class DatasetDownloadDialogComponent implements OnInit {

    @Input() config: DatasetDownloadDialogConfig;
    dataset: Dataset;
    datasetUrl: string;

    constructor(
      private modal: ModalDialogService,
      private toasterService: ToasterService) {
    }

    ngOnInit() {
        if (!this.config) {
            console.log('no config');
            return;
        }

        this.datasetUrl = this.config.datasetUrl;
        this.dataset = this.config.dataset;
    }

    public onClose(closeValue: boolean): void {
        this.modal.close({});
    }

    onUrlCopy() {
        this.toasterService.pop('success', 'Success', 'Copied Url');
    }
}
