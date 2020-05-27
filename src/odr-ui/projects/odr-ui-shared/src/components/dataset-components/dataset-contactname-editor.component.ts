import {Component, Input} from '@angular/core';
import {FormControl, FormGroup, Validators} from '@angular/forms';

export const datasetContactNameFormControl = () => new FormControl(
    '',
    [Validators.required, Validators.maxLength(128)]
);

@Component({
    selector: 'app-dataset-contact-name-editor',
    template: `
        <app-validated-input [inputControl]="control" [size]="size" labelText="Name" [required]="true">
            <app-errmsg errorCode="required">
                Name is required.
            </app-errmsg>
            <app-errmsg errorCode="maxlength">
                Name cannot be greater than {{controlErrors('maxlength').requiredLength}} characters.
            </app-errmsg>
        </app-validated-input>
    `
})
export class DatasetContactNameEditorComponent {
    @Input() control: FormControl;
    @Input() size: number;

    controlErrors(errorCode: string) {
        return this.control.getError(errorCode) || {};
    }
}
