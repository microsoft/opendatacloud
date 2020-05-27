import {Component, Input} from '@angular/core';
import {FormControl, FormGroup, Validators} from '@angular/forms';

export const datasetDescriptionFormControl = () => new FormControl(
    '',
    [Validators.required, Validators.maxLength(5000)]
);

@Component({
    selector: 'app-dataset-description-editor',
    template: `
        <app-validated-textarea [inputControl]="control" [size]="size" labelText="Description" [required]="true">
            <app-errmsg errorCode="required">
                Dataset description is required.
            </app-errmsg>
            <app-errmsg errorCode="maxlength">
                Dataset description cannot be greater than {{controlErrors('maxlength').requiredLength}} characters.
            </app-errmsg>
        </app-validated-textarea>
    `
})
export class DatasetDescriptionEditorComponent {
    @Input() control: FormControl;
    @Input() size: number;

    controlErrors(errorCode: string) {
        return this.control.getError(errorCode) || {};
    }
}

