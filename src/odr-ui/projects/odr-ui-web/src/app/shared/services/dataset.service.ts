import { Injectable } from '@angular/core';

import { Observable ,  BehaviorSubject } from 'rxjs';

import { OdrService } from './odr.service';
import { Dataset } from '../types';
import { DatasetEdit } from 'odr-ui-shared';

/**
 * Manage datasets
 */
@Injectable()
export class DatasetService {
    /**
     * Constructor
     */
    constructor(private odrService: OdrService) {}

    // Public Methods

    /**
     * Retrieve dataset
     * @param id
     */
    public getDetailDataset(id?: string): Observable<Dataset> {
        return this.odrService.getDatasetById(id);
    }

    public getDatasetEdit(id?: string): Observable<DatasetEdit> {
      return this.odrService.getDatasetEditById(id);
  }
}
