// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { buildCacheStream } from '../utils/cache-stream';

@Injectable()
export class DomainsService {

    private domainsListCache: () => Observable<any[]>;
    private baseUrl: string;

    constructor(
        @Inject('API_BASE_URL') apiBaseUrl: string,
        private http: HttpClient) {
        this.baseUrl = apiBaseUrl;

        this.domainsListCache = buildCacheStream<any>(this.http.get(`${this.baseUrl}domains`));
    }

    public getDomainsList(): Observable<any[]> {
        return this.domainsListCache();
    }
}
