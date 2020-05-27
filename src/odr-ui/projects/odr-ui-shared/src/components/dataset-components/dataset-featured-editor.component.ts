import { Component, Input } from "@angular/core";
import { FormControl, AbstractControl } from "@angular/forms";

export const datasetFeaturedFormControl = () => new FormControl("", []);

@Component({
  selector: "app-dataset-featured-editor",
  template: `
    <div class="form-group featured" [formGroup]="formGroup">
      <label>
        <input type="checkbox" [formControl]="control" />
        Feature this Dataset
      </label>
    </div>
  `,
  styles: [
    `
      .featured label {
        margin-left: 2rem;
      }
    `
  ]
})
export class DatasetFeaturedEditorComponent {
  @Input() control: FormControl;

  get formGroup(): AbstractControl {
    return this.control.parent;
  }
}
