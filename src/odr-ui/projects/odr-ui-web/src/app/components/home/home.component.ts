// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription, Observable, merge, pipe } from 'rxjs';
import { OdrService } from '../../shared/services';
import { Constraints, Dataset, DatasetQueryParams } from '../../shared/types';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { ToasterService } from 'angular2-toaster';
import { catchError, isEmpty } from 'rxjs/operators';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.scss']
})

export class HomeComponent {

    public featuredDatasets: Observable<Dataset[]>;
    public form: FormGroup;
    public constraints: Constraints = new Constraints();

    public formErrors: any = {
        terms: ''
    };

    // Messages used to validate
    private validationMessages = {
        'terms': {
            'maxlength': `Name cannot be more than ${this.constraints.termLength} characters long.`
        }
    };

    constructor(
        private fb: FormBuilder,
        private odrService: OdrService,
        private toasterService: ToasterService,
        private router: Router) {

        this.form = this.fb.group({
            terms: ['', [Validators.maxLength(this.constraints.termLength)]],
        });

        this.featuredDatasets = this.odrService.getFeaturedDatasets()
          .pipe(
            catchError((err) => {
              console.error(err);
              return [];
            })
          );
    }

    public loadCategory(categoryId: string) {
        const term = this.form.get('terms').value;
        const datasetQueryParams: DatasetQueryParams = { domain: categoryId.toUpperCase() };
        if (term) {
            datasetQueryParams.term = term;
        }
        this.router.navigate(['/datasets'], { queryParams: datasetQueryParams });
    }

    public selectResult(result: Dataset): void {
        this.router.navigate(['/datasets', result.id]);
    }

    public onSubmit(): void {
        if (this.form.valid) {
            const terms = this.form.get('terms').value;
            const datasetQueryParams: DatasetQueryParams = { term: terms };
            this.router.navigate(['/datasets'], { queryParams: datasetQueryParams });
        } else {
            this.toasterService.pop('warning', 'Warning', 'Cannot perform new dataset search with invalid terms.');
        }
    }
}
