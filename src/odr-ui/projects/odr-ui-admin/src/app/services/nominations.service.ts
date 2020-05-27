import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { DatasetNomination } from 'odr-ui-shared';
import { DatasetStorage } from '../models/dataset-storage.type';
import { forOwn } from 'lodash';

interface SearchNominationsCritera {
    page: Number;
}

@Injectable()
export class NominationsService {

    constructor(private http: HttpClient) {
    }

    public allNominations(page: number = 1): Observable<any> {
        const params = new HttpParams()
            .set('page', page.toString());
        return this.http.get(`/api/dataset-nominations`, {
            params
        });
    }

    public searchNominations({
        page = 1
    }: SearchNominationsCritera = <any>{}): Observable<any> {
        const body = {
            page
        };
        return this.http.post(`/api/dataset-nominations/search`, body);
    }

    public getDatasetNomination(id: string): Observable<DatasetNomination> {
        return this.http.get<DatasetNomination>(`/api/dataset-nominations/${id}`);
    }

    public getDatasetNominationOtherLicenseFile(id: string): Observable<any> {
       return this.http.get(`/api/dataset-nominations/${id}/other-license-file`);
    }

    public getDatasetStorage(id: string): Observable<DatasetStorage> {
        return this.http.get<DatasetStorage>(`/api/dataset-nominations/${id}/storage`);
    }

    public createDatasetStorage(storage: DatasetStorage): Observable<any> {
        const { id } = storage;
        return this.http
            .post(`/api/dataset-nominations/${id}/create-storage`, storage);
    }

    public importDatasetFromStorage(nomination: DatasetNomination): Observable<any> {
        const { id } = nomination;
        return this.http
            .post(`/api/dataset-nominations/${id}/import-from-storage`, {});
    }

    public approveDatasetNomination(nomination: DatasetNomination): Observable<any> {
        const { id } = nomination;

        const formData = this.createFormDataForPayload(nomination);

        return this.http
            .put(`/api/dataset-nominations/${id}/approve`, formData);
    }

    public rejectDatasetNomination(nomination: DatasetNomination): Observable<any> {
        const { id } = nomination;
        return this.http
            .put(`/api/dataset-nominations/${id}/reject`, nomination);
    }

    public updateDatasetNomination(nomination: DatasetNomination): Observable<any> {
        const { id } = nomination;

        const formData = this.createFormDataForPayload(nomination);

        return this.http.put(`/api/dataset-nominations/${id}`, formData);
    }

    public createDatasetNomination(nomination: DatasetNomination): Observable<any> {
        const formData = this.createFormDataForPayload(nomination);

        return this.http
            .post(`/api/dataset-nominations`, formData);
    }

    public getAssetsAuthToken(): Observable<any> {
        return this.http.get(`/api/assets/auth-token`);
    }

    private createFormDataForPayload(payload: any): FormData {
        const formData = new FormData();
        forOwn(payload, (value: any, key) => {
            if (value) {
                // if this is a Date, need to format so .NET ModelBinder can correctly interpret as date
                const finalValue = (value instanceof Date) ? new Date(value).toUTCString() : value;
                formData.append(`nomination.${key}`, finalValue);
            }
        });
        return formData;
    }
}
