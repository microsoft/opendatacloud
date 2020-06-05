// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

export const datasetPublishedFormControl = () => new FormControl(
    '',
    []
);

const style = `
    label {
        width: 100%;
    }
    label input {
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
    selector: 'app-dataset-published-editor',
    template: `
        <div class="form-group" [ngClass]="rootClasses">
            <label class="control-label">
                Date Published<span class="required" *ngIf="required">*</span>
                <input type="text" class="form-control" bsDatepicker [formControl]="control" [bsConfig]="{ showWeekNumbers: false }"  />
            </label>
            <span class="help-block" *ngIf="isRequiredErrorVisible()">
                Date Published is required.
            </span>
        </div>
    `,
    styles: [style]
})
export class DatasetPublishedEditorComponent implements OnInit {
    @Input() control: FormControl;
    @Input() size: number;
    @Input() required: boolean;

    ngOnInit(): void {
        if (this.required) {
            this.control.setValidators([Validators.required]);
        }
    }

    isRequiredErrorVisible() {
        return this.control.getError('required') && this.isErrorVisible();
    }

    isErrorVisible() {
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
