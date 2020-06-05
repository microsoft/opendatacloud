// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Inject, Injectable, OnDestroy } from '@angular/core';
import {
    HttpClient, HttpParams
} from '@angular/common/http';

import { Observable, of, concat } from 'rxjs';
import { switchMap, map, catchError, merge, startWith, mergeMap } from 'rxjs/operators';
import { AuthService } from './auth.service';

import {
    AcceptLicense,
    Dataset,
    DatasetSearch,
    FileEntry,
    PageResult
} from '../types';
import { License, DatasetEdit } from 'odr-ui-shared';
import { DatasetDomainType } from '../types/dataset-domain.type';
import { FAQType } from '../types/faq.type';
import { Submission, SubmissionStatus } from '../types/submission.type';
import { DatasetNomination } from 'odr-ui-shared';
import { HttpErrorResponse } from '@angular/common/http';
import { DatasetIssue } from '../types/dataset-issue.type';
import { CreateDeployment } from '../types/create-deployment.type';
import { GeneralIssue } from '../types/general-issue.type';
import { forOwn } from 'lodash';
import { CurrentUserDetails } from '../types';

interface SasTokenDetails {
  token: string;
}

@Injectable()
export class OdrService implements OnDestroy {

    constructor(@Inject('API_BASE_URL') apiBaseUrl: string,
        private authService: AuthService,
        private http: HttpClient) {
        // let headers = new Headers();
        // headers.append('Accept', 'text/html,application/json,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8');
        // headers.append('Content-Type', 'application/json');
        // this.httpOptions = new RequestOptions({headers: headers, withCredentials: true});

        this.baseUrl = apiBaseUrl;

        // this.bearerTokenSub = this.authService.getBearerToken().subscribe((bearerToken) => {
        //   this.manageBearerToken(bearerToken);
        // });
    }

    // Private fields
    // private bearerTokenSub: Subscription;
    private baseUrl: string;
    // private httpOptions: RequestOptions;


    // Lifecycle Hooks
    ngOnDestroy(): void {
        // this.bearerTokenSub.unsubscribe();
    }

    // Private Methods
    /**
     * Handles adding and removing bearer token value from http options header collection.
     * @param bearerToken
     */
    // private manageBearerToken(bearerToken: string): void {
    //   if (bearerToken) {
    //     this.httpOptions.headers.append('Authorization', 'Bearer ' + bearerToken);
    //   } else {
    //     this.httpOptions.headers.delete('Authorization');
    //   }
    // }

    // Public Methods

    // Dataset

    public getBaseUrl(): string {
        return this.baseUrl;
    }

    public get nameSearchUrl() {
        return `${this.baseUrl}datasets/name-search?name=`;
    }

    /**
     * Gets the dataset by its identifier.
     */
    public getDatasetById(id: string): Observable<Dataset> {
        return this.http.get<Dataset>(`${this.baseUrl}datasets/${id}`);
    }

    /**
     * Gets the dataset edit by its identifier.
     */
    public getDatasetEditById(id: string): Observable<DatasetEdit> {
        return this.http.get<DatasetEdit>(`${this.baseUrl}dataset-edits/${id}`);
    }

    /**
     * Publish the current dataset edit changes.
     */
    public modifyDatasetContent(id: string): Observable<DatasetEdit> {
      return this.http.put<DatasetEdit>(`${this.baseUrl}dataset-edits/${id}/update-content`, {});
    }

    /**
     * Publish the current dataset edit changes.
     */
    public publishDatasetEditChanges(id: string): Observable<boolean> {
        return this.http.put<boolean>(`${this.baseUrl}dataset-edits/${id}/commit`, {});
    }

    /**
     * Cancel any dataset edit changes.
     */
    public cancelDatasetEditChanges(id: string): Observable<boolean> {
        return this.http.delete<boolean>(`${this.baseUrl}dataset-edits/${id}`);
    }

    /**
     * Get SAS token for dataset edit updated content.
     */
    public getDatasetEditUpdatedContentToken(id: string): Observable<SasTokenDetails> {
        return this.http.get<SasTokenDetails>(`${this.baseUrl}dataset-edits/${id}/update-sas-token`);
    }

    /**
     * Get SAS token for dataset edit original content.
     */
    public getDatasetEditOriginalContentToken(id: string): Observable<SasTokenDetails> {
        return this.http.get<SasTokenDetails>(`${this.baseUrl}dataset-edits/${id}/original-sas-token`);
    }

    /**
     * Gets the download URI for the complete dataset
     */
    public getDatasetDownloadUrl(id: string): Observable<string> {
        return this.http.get<string>(`${this.baseUrl}datasets/${id}/url`);
    }

    /**
     * Gets the available datasets
     */
    public getDatasets(): Observable<PageResult<Dataset>> {
        return this.http.get<PageResult<Dataset>>(`${this.baseUrl}datasets`);
    }

    /**
    * Gets the featured datasets
    */
    public getFeaturedDatasets(quantity: number = 5): Observable<Dataset[]> {
        return this.http.get<Dataset[]>(`${this.baseUrl}datasets/featured/${quantity}`);
    }

    /**
     * Searches the dataset using the specified criteria.
     */
    public getDatasetSearchResults(payload: DatasetSearch): Observable<PageResult<Dataset>> {
        return this.http.post<PageResult<Dataset>>(`${this.baseUrl}datasets/search`, payload);
    }

    /**
     * Sets the acceptance status of the license for the dataset.
     */
    public getCurrentUserDetails(): Observable<CurrentUserDetails> {
        return this.http.get<CurrentUserDetails>(`${this.baseUrl}user/current`);
    }

    /**
     * Sets the acceptance status of the license for the dataset.
     */
    public setDatasetAcceptLicense(datasetId: string, payload: AcceptLicense): Observable<string> {
        return this.http.post<string>(`${this.baseUrl}user/accept-license/${datasetId}`, payload);
    }

    /**
     * Gets the acceptance status of the license for the dataset.
     */
    public getDatasetLicenseStatus(datasetId: string): Observable<boolean> {
        return this.http.get<boolean>(`${this.baseUrl}user/get-license/${datasetId}`);
    }

    /**
     * Sets the acceptance status of the license for the dataset.
     */
    public createDeployment(payload: CreateDeployment): Observable<string> {
        return this.http.post<string>(`${this.baseUrl}azure-deploy/create`, payload);
    }


    // Files

    /**
     * Gets the file by its identifier.
     */
    public getDatasetFileById(datasetId: string, id: string): Observable<PageResult<Dataset>> {
        return this.http.get<PageResult<Dataset>>(`${this.baseUrl}datasets/${datasetId}/files/${id}`);
    }

    /**
     * Gets the preview for the specified file.
     */
    public getDatasetFilePreview(datasetId: string, id: string): Observable<any> {
        const previewUrl = this.baseUrl + `datasets/${datasetId}/files/${id}/preview`;
        return this.http.get(previewUrl, {
            responseType: 'text'
        });
    }

    /**
     * Gets the direct download URL for the file.
     */
    public getDatasetFileDownloadUrl(datasetId: string, id: string): Observable<string> {
        return this.http.get<string>(`${this.baseUrl}datasets/${datasetId}/files/${id}/url`);
    }

    /**
     * Gets the direct download URL for the zip file.
     */
    public getZipFileDownloadUrl(datasetId: string): Observable<string> {
        return this.http.get<string>(`${this.baseUrl}datasets/${datasetId}/zip`);
    }

    /**
     * Gets the direct download URL for the gzip file.
     */
    public getGzipFileDownloadUrl(datasetId: string): Observable<string> {
        return this.http.get<string>(`${this.baseUrl}datasets/${datasetId}/gzip`);
    }

    /**
     * Gets the files associated with the specified dataset.
     */
    public getDatasetFiles(datasetId: string, page?: number, folder?: string): Observable<PageResult<FileEntry>> {
        const params = new HttpParams()
            .set('datasetId', datasetId)
            .set('folder', folder)
            .set('page', page.toString());
        return this.http.get<PageResult<FileEntry>>(`${this.baseUrl}datasets/${datasetId}/files`, {
            params
        });
    }

    // File Types

    /**
     * Provides access to the available file types from the datasets.
     */
    public getFileTypes(): Observable<Array<string>> {
        return this.http.get<Array<string>>(`${this.baseUrl}file-types/available`);
    }

    /**
     * Provides access to the previewable file types.
     */
    public getPreviewFileTypes(): Observable<Array<string>> {
        return this.http.get<Array<string>>(`${this.baseUrl}file-types/preview`);
    }

    // Licenses

    /**
     * Gets the license by its identifier.
     */
    public getLicenseById(id: string): Observable<License> {
        return this.http.get<License>(`${this.baseUrl}licenses/${id}`);
    }

    /**
     * Gets the content by identifier.
     */
    public getLicenseContentById(id: string): Observable<string> {
        return this.http.get<string>(`${this.baseUrl}licenses/${id}/content`);
    }

    /**
     * Gets the content by url.
     */
    public getLicenseContentByUrl(url: string): Observable<string> {
        return this.http.get<string>(`${this.baseUrl}${url}`);
    }

    /**
     * Gets the available licenses.
     */
    public getLicenses(): Observable<License[]> {
        return this.http.get<License[]>(`${this.baseUrl}licenses/standard`);
    }

    // Tags

    /**
     * Gets the available tags.
     */
    public getTags(): Observable<Array<string>> {
        return this.http.get<Array<string>>(`${this.baseUrl}tags`);
    }

    // Get domains only currently in use by Datasets
    public getDomainsInUseByDatasets(): Observable<DatasetDomainType[]> {
        return this.http.get<DatasetDomainType[]>(`${this.baseUrl}domains/active`);
    }

    // Get list of FAQs
    public getFAQs(): Observable<FAQType[]> {
        return this.http.get<FAQType[]>(`${this.baseUrl}faqs`);
    }

    // Post nomination
    public submitDatasetNomination() {
      return (source: Observable<DatasetNomination>): Observable<Submission> => {
        return source.pipe(
          switchMap((nomination) => {
            const initialStream = of({
              status: SubmissionStatus.submitting
            });

            const postStream = of(nomination).pipe(
              map(nomination => this.createFormDataForPayload("nomination", nomination)),
              mergeMap(formData => this.http.post(`${this.baseUrl}dataset-nominations`, formData)),
              map(response => ({
                status: SubmissionStatus.success,
                result: response
              })),
              catchError((response: HttpErrorResponse) => this.handleError(response))
            );

            return concat(initialStream, postStream);
          }),
        );
      }
    }

    // Post dataset edit
    public submitDatasetEdit() {
      return (source: Observable<DatasetEdit>): Observable<Submission> => {
        return source.pipe(
          switchMap((dataset) => {
            const initialStream = of({
              status: SubmissionStatus.submitting
            });

            const postStream = of(dataset).pipe(
              map(dataset => {
                const {id} = dataset;
                return [id, this.createFormDataForPayload("update", dataset)];
              }),
              switchMap(([id, formData]) => this.http.put(`${this.baseUrl}dataset-edits/${id}`, formData)),
              map(response => ({
                status: SubmissionStatus.success,
                result: response
              })),
              catchError((response: HttpErrorResponse) => this.handleError(response))
            );

            return concat(initialStream, postStream);
          })
        );
      }
    }

    // Post dataset issue
    public submitDatasetIssue(issueObservable: Observable<DatasetIssue>): Observable<Submission> {
        const submitStream = issueObservable
            .pipe(
                switchMap((issue) => {
                    // console.log(JSON.stringify(issue, null, 2));
                    const inProgressStream = of({
                        status: SubmissionStatus.submitting
                    });
                    const httpStream = this.http
                        .post(this.baseUrl + `dataset-issues`, issue)
                        .pipe(
                            map(response => ({
                                status: SubmissionStatus.success,
                                result: response
                            })),
                            catchError((response: HttpErrorResponse) => this.handleError(response))
                        );

                    return inProgressStream.pipe(merge(httpStream));
                })
            );

        return of({ status: SubmissionStatus.none }).pipe(merge(submitStream));
    }

    // Post general issue
    public submitGeneralIssue(issueObservable: Observable<GeneralIssue>): Observable<Submission> {
        const submitStream = issueObservable
            .pipe(
                switchMap((issue) => {
                    // console.log(JSON.stringify(issue, null, 2));
                    const inProgressStream = of({
                        status: SubmissionStatus.submitting
                    });
                    const httpStream = this.http
                        .post(this.baseUrl + `feedback`, issue)
                        .pipe(
                            map(response => ({
                                status: SubmissionStatus.success,
                                result: response
                            })),
                            catchError((response: HttpErrorResponse) => this.handleError(response))
                        );
                    return inProgressStream.pipe(merge(httpStream));
                })
            );

        return of({ status: SubmissionStatus.none }).pipe(merge(submitStream));
    }

    private handleError(response: HttpErrorResponse): Observable<Submission> {
        return of({
            status: SubmissionStatus.error,
            error: response.error ? response.error : {
                message: response.message || 'Error'
            }
        });
    }

    private createFormDataForPayload(prefix: string, payload: any): FormData {
        const formData = new FormData();
        forOwn(payload, (value: any, key) => {
            if (value) {
                // if this is a Date, need to format so .NET ModelBinder can correctly interpret as date
                const finalValue = (value instanceof Date) ? new Date(value).toUTCString() : value;
                formData.append(`${prefix}.${key}`, finalValue);
            }
        });
        return formData;
    }
}
