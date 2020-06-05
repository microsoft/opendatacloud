// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { BehaviorSubject, combineLatest, Observable, of, Subject } from 'rxjs';
import { catchError, map, mergeMap, switchMap, tap } from 'rxjs/operators';
import { AuthService, DatasetSchemaService, DatasetService, OdrService } from '../../shared/services';
import { Dataset } from '../../shared/types/dataset.type';

interface DatasetLicenseUserDetails {
    dataset: Dataset;
    acceptedLicense: boolean;
    isAuthenticated: boolean;
}

@Component({
    selector: 'app-dataset-loader',
    templateUrl: './dataset-loader.component.html',
})
export class DatasetLoaderComponent implements OnDestroy {

    public isLoading = true;
    public notFound = false;
    public datasetDetails: Observable<DatasetLicenseUserDetails>;

    private refreshSubject: Subject<boolean> = new BehaviorSubject(true);

    constructor(
        private route: ActivatedRoute,
        private authService: AuthService,
        private datasetService: DatasetService,
        private datasetSchemaService: DatasetSchemaService,
        private odrService: OdrService) {

        const refreshStream = this.refreshSubject.asObservable();

        const datasetIdStream = this.route.params
            .pipe(
                map((params: Params) => params['id'])
            );

        const authenticatedStream = this.authService
            .getAuthenticationStatus();

        const licenseAcceptedStream =
            combineLatest(datasetIdStream, authenticatedStream, refreshStream)
                .pipe(
                    mergeMap(([datasetId, isAuthenticated]) => {
                        return isAuthenticated ?
                            this.odrService.getDatasetLicenseStatus(datasetId).pipe(map(b => !!b)) :
                            of(false);
                    })
                );

        const datasetStream = datasetIdStream
            .pipe(
                tap(() => {
                    this.isLoading = true;
                    this.notFound = false;
                }),
                switchMap((datasetId) => this.datasetService.getDetailDataset(datasetId)),
                catchError((err) => {
                    if (err instanceof HttpErrorResponse && err.status === 404) {
                        this.notFound = true;
                    } else {
                        console.error(err);
                    }
                    return of(null);
                }),
                tap((dataset) => {
                    this.isLoading = false;
                    this.datasetSchemaService.setSchemaMetaData(dataset);
                })
            );

        this.datasetDetails =
            combineLatest(authenticatedStream, licenseAcceptedStream, datasetStream)
                .pipe(
                    map(([isAuthenticated, acceptedLicense, dataset]: [boolean, boolean, Dataset]) =>
                        dataset ? { isAuthenticated, acceptedLicense, dataset } : null)
                );
        // .do((x) => {
        //     console.log(JSON.stringify(x, null, 2));
        // });
    }

    ngOnDestroy(): void {
      this.datasetSchemaService.removeSchemaMetaData();
    }

    // Save the fact that the user accepted the license for the current dataset.
    userDidAcceptLicense = ({ datasetId, reason }: { datasetId: string, reason: string }): void => {
        this.odrService
            .setDatasetAcceptLicense(datasetId, { reason })
            .pipe(
                tap(() => {
                    this.refreshSubject.next(true);
                })
            )
            .subscribe();
    }
}
