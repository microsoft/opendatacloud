// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {
  Component,
  SecurityContext,
  OnInit,
  Input,
  AfterViewInit,
  HostListener
} from "@angular/core";
import { FormGroup, FormControl, Validators } from "@angular/forms";
import { DomSanitizer } from "@angular/platform-browser";
import { AngularEditorConfig } from "@kolkov/angular-editor";
import { ModalDialogService } from "../../services/modal-dialog.service";
import {
  LicenseEntryType,
  LicenseContentType
} from "../../models/license-entry.type";
import {
  updateValidations,
  conditionalValidator
} from "../../utils/conditional-validator";
import { tap } from "rxjs/operators";

const maxLengthForHtmlContent = 2000000;

@Component({
  selector: "app-dataset-license-dialog",
  templateUrl: "./dataset-license-dialog.component.html",
  styleUrls: ["./dataset-license-dialog.component.scss"]
})
export class DatasetLicenseDialogComponent implements OnInit, AfterViewInit {
  @Input() entry: LicenseEntryType;

  form: FormGroup;

  editorConfig: AngularEditorConfig = {
    editable: true,
    spellcheck: false,
    height: "500px",
    minHeight: "500px",
    width: "auto",
    minWidth: "0",
    placeholder: "Paste License Content here...",
    translate: "no",
    showToolbar: false,
    enableToolbar: false
  };

  selectedFile: File = null;

  constructor(
    private modal: ModalDialogService,
    private domSanitizer: DomSanitizer
  ) {}

  ngOnInit() {
    this.form = new FormGroup({
      licenseName: new FormControl(
        {
          value: this.entry.name,
          disabled: Boolean(this.entry.readOnly)
        },
        [Validators.required, Validators.maxLength(256)]
      ),
      licenseUrl: new FormControl(
        {
          value: this.entry.additionalInfoUrl,
          disabled: Boolean(this.entry.readOnly)
        },
        [Validators.maxLength(1024)]
      ),
      isFileUpload: new FormControl({
        value: this.entry.contentType === LicenseContentType.InputFile,
        disabled: Boolean(this.entry.readOnly)
      }),
      contentHtml: new FormControl(
        {
          value: this.entry.contentHtml,
          disabled: false
        },
        [
          Validators.maxLength(maxLengthForHtmlContent),
          conditionalValidator(
            () => !this.isFileUpload.value,
            Validators.required
          )
        ]
      ),
      fileInput: new FormControl(
        {
          value: null,
          disabled: Boolean(this.entry.readOnly)
        },
        [
          conditionalValidator(
            () => this.isFileUpload.value,
            Validators.required
          )
        ]
      )
    });

    updateValidations(this.form, "isFileUpload", ["contentHtml", "fileInput"]);

    this.isFileUpload.valueChanges
      .pipe(
        tap(isFileUpload => {
          this.fileInput.setValue(null);
        })
      )
      .subscribe();
  }

  ngAfterViewInit(): void {
    const { errors } = this.entry;
    if (Array.isArray(errors) && errors.length > 0) {
      Object.entries(this.form.controls)
        .map(([name, control]) => {
          const entries = errors.filter(({ propertyName }) => {
            return propertyName.toLowerCase() === name.toLowerCase();
          });
          return { control, entries };
        })
        .filter(({ entries }) => entries.length > 0)
        .forEach(({ control, entries }) => {
          const clientErrors = entries.reduce(
            (errObj, { errorCode, errorMessage }) => {
              errObj[errorCode] = {
                message: errorMessage
              };
              return errObj;
            },
            {}
          );
          control.setErrors(clientErrors, { emitEvent: true });
          control.markAsDirty();
        });
    }
  }

  get licenseName(): FormControl {
    return this.form.get("licenseName") as FormControl;
  }

  get licenseUrl(): FormControl {
    return this.form.get("licenseUrl") as FormControl;
  }

  get isFileUpload(): FormControl {
    return this.form.get("isFileUpload") as FormControl;
  }

  get contentHtml(): FormControl {
    return this.form.get("contentHtml") as FormControl;
  }

  get fileInput(): FormControl {
    return this.form.get("fileInput") as FormControl;
  }

  get isReadOnly(): boolean {
    return Boolean(this.entry.readOnly);
  }

  get isFormValid(): boolean {
    return this.form.valid;
  }

  get fileName() {
    return this.entry.fileName;
  }

  get otherLicenseFileUrl() {
    return this.entry.id && this.entry.fileName
      ? `/api/dataset-nominations/${this.entry.id}/other-license-file`
      : null;
  }

  onFileChange(event) {
    const {files} = event.target;
    this.selectedFile = files && files.item(0) ? files.item(0) : null;
    if(this.selectedFile) {
      this.fileInput.setValue(this.selectedFile.name);
    }
  }

  onSave() {
    let updatedEntry: LicenseEntryType;
    if (this.isFileUpload.value) {
      updatedEntry = {
        contentType: LicenseContentType.InputFile,
        name: this.licenseName.value,
        additionalInfoUrl: this.licenseUrl.value,
        fileInput: this.selectedFile,
        fileName: this.selectedFile.name
      };
    } else {
      updatedEntry = {
        contentType: LicenseContentType.HtmlText,
        name: this.licenseName.value,
        additionalInfoUrl: this.licenseUrl.value,
        contentHtml: this.domSanitizer.sanitize(
          SecurityContext.HTML,
          this.contentHtml.value
        )
      };
    }
    this.modal.close(updatedEntry);
  }

  onCancel() {
    this.modal.close(this.entry);
  }
}
