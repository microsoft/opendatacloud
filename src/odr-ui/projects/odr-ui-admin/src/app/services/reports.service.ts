// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ReportItem } from '../models/report-item.model';

@Injectable()
export class ReportsService {

    constructor(private http: HttpClient) {
    }

    public getDatasetsByDomain(): Observable<ReportItem[]> {
        return this.http.get<ReportItem[]>(`/api/reports/datasets/domain`);
    }

    public getDatasetsByLicense(): Observable<ReportItem[]> {
        return this.http.get<ReportItem[]>(`/api/reports/datasets/license`);
    }

    public getDatasetPageViews(): Observable<ReportItem[]> {
        return this.http.get<ReportItem[]>(`/api/reports/datasets/page-views`);
    }

    public getDatasetAzureDeployments(): Observable<ReportItem[]> {
        return this.http.get<ReportItem[]>(`/api/reports/datasets/azure-deployments`);
    }

    public getViewsByRegion(): Observable<ReportItem[]> {
        return this.http.get<ReportItem[]>(`/api/reports/views-by-region`);
    }

    public getViewsByDate(): Observable<ReportItem[]> {
        return this.http.get<ReportItem[]>(`/api/reports/views-by-date`);
    }

    public getSearchesByDomain(): Observable<ReportItem[]> {
        return this.http.get<ReportItem[]>(`/api/reports/searches-by-domain`);
    }

    public getSearchesByTerm(): Observable<ReportItem[]> {
        return this.http.get<ReportItem[]>(`/api/reports/searches-by-term`);
    }
}
