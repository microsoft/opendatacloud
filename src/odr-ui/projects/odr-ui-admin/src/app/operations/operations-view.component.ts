// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { BatchService } from '../services';
import { Observable, Subject, of } from 'rxjs';
import { switchMap, concat, tap } from 'rxjs/operators';

@Component({
    selector: 'app-operations-view',
    templateUrl: './operations-view.component.html',
    styleUrls: ['./operations-view.component.scss']
})
export class OperationsViewComponent implements OnInit {

    @ViewChild('outputPane', {static: false}) private outputContainer: ElementRef;

    batchDetails: Observable<any>;
    batchOutput: Observable<any>;
    selectedId: string;

    private requestOperations: Subject<any> = new Subject<any>();
    private requestOutput: Subject<any> = new Subject<any>();

    constructor(private batchService: BatchService) {
    }

    ngOnInit() {
        this.batchDetails = this.requestOperations
            .pipe(
                switchMap(() => {
                    const fetchStream = this.batchService.getLatestBatchOperations();
                    return of(null).pipe(concat(fetchStream));
                })
            );

        this.batchOutput = this.requestOutput
            .pipe(
                switchMap(({ id }) => {
                    const fetchStream = this.batchService.getBatchOutput(id)
                        .pipe(
                            tap(() => {
                                this.selectedId = id;
                                setTimeout(() => this.scrollToBottomOfOutput(), 100);
                            })
                        );
                    return (
                        of(null)
                        .pipe(
                            tap(() => {
                                this.selectedId = null;
                            }),
                            concat(fetchStream)
                        ));
                })
            );

        setTimeout(() => this.refreshData());
    }

    selectOperation({ id }) {
        this.requestOutput.next({ id });
    }

    refreshData() {
        const { selectedId: id } = this;
        this.requestOperations.next(true);
        if (id) {
            this.requestOutput.next({ id });
        }
    }

    scrollToBottomOfOutput(): void {
        const { outputContainer } = this;
        if (outputContainer && outputContainer.nativeElement) {
            outputContainer.nativeElement.scrollTop = outputContainer.nativeElement.scrollHeight;
        }
    }
}
