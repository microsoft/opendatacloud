import {Component, Input} from '@angular/core';
import {FormControl, FormGroup, Validators} from '@angular/forms';

export const datasetNameFormControl = () => new FormControl(
    '',
    [Validators.required, Validators.maxLength(128)]
);

@Component({
    selector: 'app-dataset-name-editor',
    template: `
        <app-validated-input [inputControl]="control" [size]="size" labelText="Dataset Name" [required]="true">
            <app-errmsg errorCode="required">
                Dataset name is required.
            </app-errmsg>
            <app-errmsg errorCode="maxlength">
                Dataset name cannot be greater than {{controlErrors('maxlength').requiredLength}} characters.
            </app-errmsg>
        </app-validated-input>
    `
})
export class DatasetNameEditorComponent {
    @Input() control: FormControl;
    @Input() size: number;

    controlErrors(errorCode: string) {
        return this.control.getError(errorCode) || {};
    }
}

