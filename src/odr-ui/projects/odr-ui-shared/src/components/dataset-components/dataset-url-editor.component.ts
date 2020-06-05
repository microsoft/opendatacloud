// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';

export const datasetUrlFormControl = () => new FormControl(
    '',
    [Validators.maxLength(1024), Validators.required]
);

const style = `
    .go-link { padding-top: 2.5rem; }
`;

@Component({
    selector: 'app-dataset-url-editor',
    template: `
        <app-validated-url [inputControl]="control"
            labelText="URL to Dataset (if different from Project Page URL)"
            (labelChecked)="onClickDatasetUrlDistinct()"
            [isLabelChecked]="isDatasetUrlDistinct"
            [isLabelCheckboxVisible]="true"
            [size]="size" [required]="true">
            <app-errmsg errorCode="required">
                Dataset Url is required.
            </app-errmsg>
            <app-errmsg errorCode="maxlength">
                Dataset URL cannot be greater than {{controlErrors('maxlength').requiredLength}} characters.
            </app-errmsg>
            <app-errmsg errorCode="url">
                {{controlErrors('url').message}}
            </app-errmsg>
        </app-validated-url>
    `,
    styles: [style]
})
export class DatasetUrlEditorComponent {
    @Input() control: FormControl;
    @Input() size: number;
    @Input() isDatasetUrlDistinct: boolean;
    @Output() datasetUrlDistinctChanged: EventEmitter<any> = new EventEmitter<any>();

    controlErrors(errorCode: string) {
        return this.control.getError(errorCode) || {};
    }

    onClickDatasetUrlDistinct() {
        this.datasetUrlDistinctChanged.emit();
    }
}
