// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, OnInit } from '@angular/core';
import { DatasetsService } from '../services/datasets.service';
import { Observable } from 'rxjs';
import { map, tap, switchMap } from 'rxjs/operators';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
    selector: 'app-datasets-view',
    templateUrl: './datasets-view.component.html',
    styleUrls: ['./datasets-view.component.scss']
})
export class DatasetsViewComponent implements OnInit {

    pagedDatasets: Observable<any>;
    itemsPerPage = 5;
    currentPage = 1;

    constructor(
        private datasetsService: DatasetsService,
        private route: ActivatedRoute,
        private router: Router) {
    }

    ngOnInit() {
        this.pagedDatasets = this.route.queryParams
            .pipe(
                map(({ page }) => ({
                    page: Number(page || 1)
                })),
                tap(({ page }) => {
                    this.currentPage = page;
                }),
                switchMap(({ page }) => {
                    return this.datasetsService.getDatasetsList(page);
                })
            );
    }

    pageChanged(event: any): void {
        const { page } = event;
        this.router.navigate(['datasets'], { queryParams: { page } });
    }

    editDataset(dataset) {
        const { id } = dataset;
        this.router.navigate(['datasets', id]);
    }
}
