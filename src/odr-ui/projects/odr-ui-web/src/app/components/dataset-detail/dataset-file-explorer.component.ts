// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Dataset } from '../../shared/types/dataset.type';
import { Observable, Subject, of } from 'rxjs';
import { switchMap, catchError, mergeMap, tap, map } from 'rxjs/operators';
import { PageResult } from '../../shared/types/page-result.type';
import { FileEntry } from '../../shared/types/file-entry.type';
import { OdrService } from '../../shared/services/odr.service';
import { FileEntryType } from '../../shared/types/file-entry-type.enum';
import { PreviewDialogComponent } from './preview-dialog.component';
import { ToasterService } from 'angular2-toaster';
import { ModalDialogService } from 'odr-ui-shared';

interface SelectedFolderAndPage {
    folder: string;
    pageNumber: number;
}

const FIRST_PAGE_NUMBER = 1;

const fileExtensionRegex = /\.([^.]+)$/i;

@Component({
    selector: 'app-dataset-file-explorer',
    templateUrl: './dataset-file-explorer.component.html',
    styleUrls: ['./dataset-file-explorer.component.scss']
})
export class DatasetFileExplorerComponent implements OnInit {

    @Input() dataset: Dataset;
    @Input() isAuthenticated: boolean;
    @Input() acceptedLicense: boolean;
    @Input() datasetFileDownloadUrl: string;
    @Output() onDownloadFile: EventEmitter<FileEntry> = new EventEmitter();

    displayedFiles: Observable<FileEntry[]>;
    isFileExplorerBusy = true;
    currentPageNumber = FIRST_PAGE_NUMBER;
    totalPageCount = 0;
    canNextPage = false;
    canPrevPage = false;
    currentFolders: FileEntry[] = [];
    currentFileDownload: FileEntry = null;
    previewFileTypes = new Map();

    FileEntryType = FileEntryType;

    private pagedListSubject: Subject<SelectedFolderAndPage> = new Subject();

    constructor(
        private odrService: OdrService,
        private modal: ModalDialogService,
        private toasterService: ToasterService
    ) {
        this.displayedFiles = this.pagedListSubject.asObservable()
            .pipe(
                switchMap((selection) => {
                    return of(selection)
                        .pipe(
                            tap(({ pageNumber, folder }) => {
                                this.isFileExplorerBusy = true;
                                this.currentPageNumber = pageNumber;
                            })
                        );
                }),
                mergeMap(({ pageNumber, folder }) => {
                    return this.odrService.getDatasetFiles(this.dataset.id, pageNumber, folder);
                }),
                catchError((err) => {
                    console.error(err);
                    return of(<PageResult<FileEntry>>{
                        pageCount: 0,
                        value: []
                    });
                }),
                tap(({ pageCount }) => {
                    this.isFileExplorerBusy = false;
                    this.totalPageCount = pageCount;
                    this.canPrevPage = (this.currentPageNumber > FIRST_PAGE_NUMBER);
                    this.canNextPage = (this.currentPageNumber < this.totalPageCount);
                }),
                map(({ value }) => {
                    return value.map(file => {
                        if (file.entryType === FileEntryType.file) {
                            const ext = (fileExtensionRegex.exec(file.name)[1] || '').toLowerCase();
                            file.canPreview = Boolean(this.previewFileTypes.get(ext));
                        }
                        return file;
                    });
                })
            );
    }

    ngOnInit(): void {
        setTimeout(() => {

            this.odrService
                .getPreviewFileTypes()
                .pipe(
                    tap(list => {
                        list.forEach(n => this.previewFileTypes.set(n.toLowerCase(), true));
                        this.navigateFileExplorerRoot();
                    })
                )
                .subscribe();

        }, 0);
    }

    getDownloadTitle() {
        return this.isAuthenticated ? 'Download' : 'Login to download';
    }

    navigateFileExplorerRoot() {
        this.currentFolders = [];
        this.navigateToSelected(FIRST_PAGE_NUMBER);
    }

    navigateFolderBreadcrumb(fileEntry: FileEntry) {
        const accum = {
            found: false,
            list: [],
        };
        // tslint:disable: no-shadowed-variable
        const { list } = this.currentFolders.reduce(({ found, list }, entry) => {
            if (!found) {
                list.push(entry);
                if (fileEntry === entry) {
                    found = true;
                }
            }
            return { found, list };
        }, accum);
        this.currentFolders = list;
        this.navigateToSelected(FIRST_PAGE_NUMBER);
    }

    navigateToFolder(fileEntry: FileEntry) {
        this.currentFolders = [...this.currentFolders, fileEntry];
        this.navigateToSelected(FIRST_PAGE_NUMBER);
    }

    navigateToNextPage() {
        this.navigateToSelected(this.currentPageNumber + 1);
    }

    navigateToPrevPage() {
        this.navigateToSelected(this.currentPageNumber - 1);
    }

    openPreview(file: FileEntry): void {
        const { id, name, parentFolder } = file;
        const folder = parentFolder ? `/${parentFolder}` : '';
        const config = {
            datasetId: this.dataset.id,
            id,
            name: `${folder}/${name}`,
            fileType: (fileExtensionRegex.exec(name)[1] || '').toLowerCase(),
        };

        this.modal.create({
          title: "File Preview",
          component: PreviewDialogComponent,
          params: {
            config
          }
        });

        // const modal = this.modalService.create({
        //     nzTitle: 'File Preview',
        //     nzContent: PreviewDialogComponent,
        //     nzComponentParams: {
        //         config: previewDetails
        //     },
        //     nzFooter: null
        // });

        // Return a result when closed
        // modal.afterClose.subscribe(result => console.log('[afterClose] The result is:', result));
    }

    selectFileToDownload(file: FileEntry): void {
        this.currentFileDownload = this.currentFileDownload === file ? null : file;
        if (file) {
            this.onDownloadFile.emit(file);
        }
    }

    private navigateToSelected(pageNumber: number) {
        const folder = this.currentFolders.map(f => f.name).join('/');
        this.pagedListSubject.next({ folder, pageNumber });
    }
}
