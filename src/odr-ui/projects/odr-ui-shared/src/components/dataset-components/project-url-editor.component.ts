import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';

export const projectUrlFormControl = () => new FormControl(
    '',
    [Validators.maxLength(1024), Validators.required]
);

const style = `
    .go-link { padding-top: 2.5rem; }
`;

@Component({
    selector: 'app-project-url-editor',
    template: `
        <app-validated-url [inputControl]="control" labelText="URL to Project Page"
            [size]="size" [required]="true" (inputValueChanged)="onChangeInputValue()">
            <app-errmsg errorCode="required">
                Project Page Url is required.
            </app-errmsg>
            <app-errmsg errorCode="maxlength">
                Project Page URL cannot be greater than {{controlErrors('maxlength').requiredLength}} characters.
            </app-errmsg>
            <app-errmsg errorCode="url">
                {{controlErrors('url').message}}
            </app-errmsg>
        </app-validated-url>
    `,
    styles: [style]
})
export class ProjectUrlEditorComponent {
    @Input() control: FormControl;
    @Input() size: number;
    @Output() inputValueChanged: EventEmitter<any> = new EventEmitter<any>();

    controlErrors(errorCode: string) {
        return this.control.getError(errorCode) || {};
    }

    onChangeInputValue() {
        this.inputValueChanged.emit();
    }
}
