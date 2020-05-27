import {Component, Input} from '@angular/core';
import {FormControl, Validators} from '@angular/forms';

export const datasetContactInfoFormControl = () => new FormControl(
    '',
    [Validators.required, Validators.maxLength(200)]
);

@Component({
    selector: 'app-dataset-contact-info-editor',
    template: `
        <app-validated-input [inputControl]="control" [size]="size" labelText="Email Address" [required]="true">
            <app-errmsg errorCode="required">
                Email address is required.
            </app-errmsg>
            <app-errmsg errorCode="maxlength">
                Email address cannot be greater than {{controlErrors('maxlength').requiredLength}} characters.
            </app-errmsg>
        </app-validated-input>
    `
})
export class DatasetContactInfoEditorComponent {
    @Input() control: FormControl;
    @Input() size: number;

    controlErrors(errorCode: string) {
        return this.control.getError(errorCode) || {};
    }
}
