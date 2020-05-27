import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { buildCacheStream } from '../utils/cache-stream';

@Injectable()
export class BatchService {

    constructor(private http: HttpClient) {
    }

    public getLatestBatchOperations(): Observable<any> {
        return this.http.get(`/api/batch/operations`);
    }

    public getBatchOutput(taskId: string): Observable<any> {
        return this.http.get(`/api/batch/operations/${taskId}/output`);
    }
}
