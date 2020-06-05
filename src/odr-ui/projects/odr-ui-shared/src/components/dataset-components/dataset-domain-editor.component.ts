// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { DomainsService } from '../../services/domains.service';
import { matchListId } from '../../utils/list-utils';

export const datasetDomainFormControl = () => new FormControl();

const style = `
    label {
        width: 100%;
    }
    label select {
        font-weight: normal;
    }
    label span.required {
        color: #a94442;
        padding-left: 3px;
        font-size: 1.3em;
        font-weight: bold;
    }
`;

@Component({
    selector: 'app-dataset-domain-editor',
    template: `
        <div class="form-group" [ngClass]="rootClasses">
            <label class="control-label">
                Dataset Domain<span class="required" *ngIf="required">*</span>
                <select class="form-control" [formControl]="control" [compareWith]="matchDomainId">
                    <option *ngFor="let domain of domains | async" [ngValue]="domain">
                        {{domain.id}} - {{domain.name}}
                    </option>
                </select>
            </label>
            <span class="help-block" *ngIf="isRequiredErrorVisible()">
                Domain is required.
            </span>
        </div>
    `,
    styles: [style]
})
export class DatasetDomainEditorComponent implements OnInit {

    @Input() control: FormControl;
    @Input() size: number;
    @Input() required: boolean;

    domains: Observable<any>;

    matchDomainId = matchListId;

    constructor(
        private domainsService: DomainsService,
    ) {
    }

    ngOnInit(): void {
        this.domains = this.domainsService.getDomainsList();

        if (this.required) {
            this.control.setValidators([Validators.required]);
        }
    }

    isRequiredErrorVisible(): boolean {
        return this.control.getError('required') && this.isErrorVisible();
    }

    isErrorVisible(): boolean {
        return !this.control.pristine && !this.control.valid;
    }

    get rootClasses() {
        let classes = this.size ? `col-lg-${this.size}` : 'col-lg-12';

        if (this.isErrorVisible()) {
            classes += ' has-error';
        }

        return classes;
    }
}
