import {Component, Input} from '@angular/core';
import {FormControl} from '@angular/forms';

export const datasetFileTypesFormControl = () => new FormControl();

@Component({
    selector: 'app-dataset-filetypes-editor',
    template: `
        <div class="form-group col-lg-6">
            <label class="control-label">
                Dataset File Types
            </label>
            <div *ngFor="let fileType of control.value" class="filetype">
                <button type="button" class="btn btn-default btn-sm" [disabled]="true" *ngIf="fileType">
                    {{fileType}}
                </button>
            </div>
        </div>
    `,
    styleUrls: ['./dataset-filetypes-editor.component.scss']
})
export class DatasetFileTypesEditorComponent {

    @Input() control: FormControl;
}
