import { Component, SecurityContext, OnInit, Input } from '@angular/core';
import { FormGroup, FormControl, Validators, AbstractControl } from '@angular/forms';
import { NominationLicenseDialogConfig } from '../../models/nomination-license-dialog-config.type';
import { DomSanitizer } from '@angular/platform-browser';
import { AngularEditorConfig } from '@kolkov/angular-editor';
import { ModalDialogService } from '../../services/modal-dialog.service';

const maxLengthForHtmlContent = 2000000;

@Component({
    selector: 'app-license-dialog',
    styleUrls: ['./nomination-license-dialog.component.scss'],
    templateUrl: './nomination-license-dialog.component.html'
})
export class NominationLicenseDialogComponent implements OnInit {
    @Input() config: NominationLicenseDialogConfig;

    nominationId: string;
    form: FormGroup;
    contentHtml: any;
    file: any;
    fileName: string;
    isFileUpload = false;
    isReadOnly = false;

    editorConfig: AngularEditorConfig = {
        editable: true,
        spellcheck: false,
        height: '500px',
        minHeight: '500px',
        width: 'auto',
        minWidth: '0',
        placeholder: 'Paste License Content here...',
        translate: 'no',
        showToolbar: false,
        enableToolbar: false
      };

    constructor(
        private modal: ModalDialogService,
        private domSanitizer: DomSanitizer) {
    }

    ngOnInit() {

        if (!this.config) {
            console.log('no config');
            return;
        }

        this.nominationId = this.config.nominationId;
        this.contentHtml = this.config.contentHtml;
        this.fileName = this.config.fileName;
        this.isFileUpload = this.config.isFileUpload;
        this.isReadOnly = this.config.isReadOnly;

        if (this.isFileUpload && this.config.file) {
            this.file = this.config.file;
        }

        if (this.config.errors) {
            setTimeout(() => {
                this.addServerErrors(this.config.errors);
            }, 100);
        }

        this.form = new FormGroup({
            licenseName: new FormControl(
                { value: this.config.name, disabled: this.isReadOnly },
                [Validators.required, Validators.maxLength(256)]
            ),
            licenseUrl: new FormControl(
                { value: this.config.additionalInfoUrl, disabled: this.isReadOnly },
                [Validators.maxLength(1024)]
            )
        });
    }

    get licenseName(): FormControl { return this.form.get('licenseName') as FormControl; }

    get licenseUrl(): FormControl { return this.form.get('licenseUrl') as FormControl; }

    get controls() {
        return this.form.controls;
    }

    get otherLicenseFileUrl() {
        if (this.isFileUpload && this.config.fileName) {
            return `/api/dataset-nominations/${this.nominationId}/other-license-file`;
        }
        return null;
    }

    public toggleFileUpload(value: boolean) {
        if (value !== this.isFileUpload) {
            this.isFileUpload = value;
            this.fileName = null;
            this.file = null;
            this.contentHtml = null;
        }
    }

    public fileChange($event) {
        const files: FileList = $event.target.files;
        this.file = files[0];
        this.fileName = this.file && this.file.name;
    }

    // Before dismissing dialog do a check
    public beforeDismiss(): boolean {
        return false;
    }

    // Before close action is fired do a check.
    public beforeClose(): boolean {
        return false;
    }

    isFileUploadRequiredErrorVisible(): boolean {
        if (!this.isFileUpload) {
            return false;
        }

        if (!this.isFileUploadDirty()) {
            return false;
        }

        return !this.file && !this.fileName;
    }

    isContentHtmlRequiredErrorVisible(): boolean {
        if (this.isFileUpload) {
            return false;
        }

        if (!this.isContentHtmlDirty()) {
            return false;
        }

        return !this.contentHtml;
    }

    isContentHtmlMaxLengthErrorVisible(): boolean {
        if (this.isFileUpload) {
            return false;
        }

        if (!this.isContentHtmlDirty()) {
            return false;
        }

        if (!this.contentHtml) {
            return false;
        }

        return this.contentHtml.length > maxLengthForHtmlContent;
    }

    public isFormDirty(): boolean {
        if (this.licenseName.touched && (this.licenseName.value !== this.config.name)) {
            return true;
        }

        if (this.licenseUrl.touched && this.licenseUrl.value !== this.config.additionalInfoUrl) {
            return true;
        }

        if (this.isFileUpload && (this.fileName !== this.config.fileName)) {
            return true;
        }

        if (!this.isFileUpload && (this.contentHtml !== this.config.contentHtml)) {
            return true;
        }

        return false;
    }

    public isValidForSave(): boolean {
        const isDirty = this.isFormDirty();
        if (!isDirty) {
            return false;
        }

        if (this.isFileUpload) {
            if (this.fileName) {
                return true;
            }

            // error need a file or that there was one previously uploaded
            // show an error
            return false;
        }

        if (this.contentHtml) {
            return true;
        }

        return false;
    }

    public onSave(): void {
        if (!this.isFormDirty()) {
            console.log('form is not dirty - cannot save');
        }

        if (!this.isValidForSave()) {
            console.log('errors - cannot save');
            return;
        }

        const result: NominationLicenseDialogConfig = new NominationLicenseDialogConfig();
        result.name = this.licenseName.value;
        result.additionalInfoUrl = this.licenseUrl.value;
        result.isFileUpload = this.isFileUpload;
        result.fileName = this.fileName;

        if (this.contentHtml) {
            const sanitized = this.domSanitizer.sanitize(SecurityContext.HTML, this.contentHtml);
            result.contentHtml = sanitized;
        }

        if (this.file) {
            result.file = this.file;
        }

        this.modal.close(result);
    }

    public onCancel(): void {
        const result: NominationLicenseDialogConfig = new NominationLicenseDialogConfig();
        this.modal.close(result);
    }

    public controlErrors(control: FormControl, errorCode: string) {
        return control.getError(errorCode) || {};
    }

    private isContentHtmlDirty() {
        return this.contentHtml !== this.config.contentHtml;
    }

    private isFileUploadDirty() {
        return this.file !== this.config.file ||
            this.fileName !== this.config.fileName;
    }

    private addServerErrors(errors: any[]): void {
        if (errors.length === 0) {
            return;
        }

        const controlsToValidate = Object.keys(this.form.controls);
        controlsToValidate.forEach(controlName => {
            const control = this.form.get(controlName);
            if (control) {
                const controlErrors = errors.filter(e => e.propertyName.toLowerCase() === controlName.toLowerCase());
                if (controlErrors && controlErrors.length > 0) {
                    const clientErrors = { valid: false, invalid: true };
                    controlErrors.map(e => clientErrors[e.errorCode] = { 'message': e.errorMessage });
                    control.setErrors(clientErrors);
                }
            }
        });
    }
}
