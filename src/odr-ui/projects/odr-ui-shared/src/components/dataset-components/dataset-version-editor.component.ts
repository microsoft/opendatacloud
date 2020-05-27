import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

export const datasetVersionFormControl = () => new FormControl(
    '',
    [Validators.maxLength(50)]
);

@Component({
    selector: 'app-dataset-version-editor',
    template: `
        <app-validated-input [inputControl]="control" [size]="size" labelText="Version" [required]="required">
            <app-errmsg errorCode="required">
                Version is required.
            </app-errmsg>
            <app-errmsg errorCode="maxlength">
                Version cannot be greater than {{controlErrors('maxlength').requiredLength}} characters.
            </app-errmsg>
        </app-validated-input>
    `
})
export class DatasetVersionEditorComponent implements OnInit {
    @Input() control: FormControl;
    @Input() size: number;
    @Input() required: boolean;

    ngOnInit(): void {
        if (this.required) {
            this.control.setValidators([Validators.maxLength(50), Validators.required]);
        } else {
            this.control.setValidators([Validators.maxLength(50)]);
        }
    }

    controlErrors(errorCode: string) {
        return this.control.getError(errorCode) || {};
    }
}
