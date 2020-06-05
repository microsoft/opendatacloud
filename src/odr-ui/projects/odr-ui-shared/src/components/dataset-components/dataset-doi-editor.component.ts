// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, Input } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';

export const datasetDoiFormControl = () => new FormControl(
    '',
    [Validators.maxLength(255)]
);

@Component({
    selector: 'app-dataset-doi-editor',
    template: `
        <app-validated-input [inputControl]="control" [size]="size" labelText="Digital Object Identifier (DOI)">
            <app-errmsg errorCode="maxlength">
                Digital Object Identifier name cannot be greater than {{controlErrors('maxlength').requiredLength}} characters.
            </app-errmsg>
        </app-validated-input>
    `
})
export class DatasetDoiEditorComponent {
    @Input() control: FormControl;
    @Input() size: number;

    controlErrors(errorCode: string) {
        return this.control.getError(errorCode) || {};
    }
}

