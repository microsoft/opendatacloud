import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Dataset } from '../models/dataset.type';

@Injectable()
export class DatasetsService {

    private datasetSubject: Subject<string>;

    constructor(private http: HttpClient) {
    }

    public getDatasetsList(page: number = 1): Observable<any> {
        const params = new HttpParams()
            .set('page', page.toString());
        return this.http.get(`/api/datasets`, {
            params
        });
    }

    public getDataset(id: string): Observable<Dataset> {
        return this.http.get<Dataset>(`/api/datasets/${id}`);
    }

    public refreshDataset(id) {
        return () => this.datasetSubject.next(id);
    }

    public updateDataset(dataset: Dataset): Observable<Dataset> {
        const { id } = dataset;
        return this.http
            .put<Dataset>(`/api/datasets/${id}`, dataset);
    }

    public deleteDataset(id: string): Observable<any> {
        return this.http
            .delete<any>(`/api/datasets/${id}`);
    }
}
