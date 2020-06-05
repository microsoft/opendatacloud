// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, OnInit, Input } from '@angular/core';
import * as XLSX from 'xlsx';
import { DatasetService, OdrService } from '../../shared/services';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ModalDialogService } from 'odr-ui-shared';
import { GridOptions } from 'ag-grid-community';

const spreadsheetTypes = new Set([
    'tsv',
    'csv'
]);

export class FileEntryPreviewConfig {
    datasetId: string;
    id: string;
    name: string;
    fileType: string;
}

@Component({
    selector: 'app-preview-dialog',
    styleUrls: ['../../shared/styles/dialog.component.css', './preview-dialog.component.scss'],
    templateUrl: './preview-dialog.component.html'
})
export class PreviewDialogComponent implements OnInit {

    @Input() config: any;

    contentText: Observable<string>;
    isBusy = true;
    gridOptions: GridOptions = {};
    rowData: any[];
    columnDefs: any[];

    constructor(
        private datasetService: DatasetService,
        private modal: ModalDialogService,
        private odrService: OdrService) {
    }

    get isTextFile() {
        return !this.isSpreadsheetFile;
    }

    get isSpreadsheetFile() {
        return spreadsheetTypes.has(this.config.fileType);
    }

    ngOnInit(): void {
        if (!this.config) {
            console.log('no config');
            return;
        }

        const { datasetId, id } = this.config;

        this.contentText = this.odrService
            .getDatasetFilePreview(datasetId, id)
            .pipe(
                tap((document) => {
                    this.isBusy = false;
                    if (this.isSpreadsheetFile) {
                        setTimeout(() => {
                            this.loadSpreadsheet(document);
                        });
                    }
                })
            );
    }

    private loadSpreadsheet(document: string) {
        const workbook = XLSX.read(document, {
            type: 'string'
        });
        const firstSheet = workbook.SheetNames[0];
        const parsed = XLSX.utils.sheet_to_json(workbook.Sheets[firstSheet]);
        this.rowData = parsed;
        this.gridOptions = {
            onGridReady: () => { }
        };
        this.columnDefs = Object.entries(parsed[0]).map(([key, value]) => ({
            headerName: key,
            field: key,
        }));
    }

    public beforeDismiss(): boolean {
        return true;
    }

    public beforeClose(): boolean {
        return false;
    }

    public onClose(closeValue?: boolean): void {
        this.modal.close(closeValue);
    }
}
