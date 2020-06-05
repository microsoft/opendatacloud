// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, OnDestroy, ViewEncapsulation } from "@angular/core";
import { FormGroup, FormControl } from "@angular/forms";
import { DatasetLicenseDialogComponent, LicenseEntryType, ModalDialogService } from "odr-ui-shared";

interface DatasetFields {
  name: string;
  description: string;
  datasetUrl: string;
  projectUrl: string;
  tags: string[];
  domain: string;
  domainId: string;
  licenseId: string;
  licenseName: string;
  published: Date;
  version: string;
  digitalObjectIdentifier: string;
  isFeatured: boolean;
  contactName: string;
  contactInfo: string;
}

export type DatasetEditFormSubmitData = [DatasetFields, LicenseEntryType];

@Component({
  selector: "app-dataset-edit-form",
  templateUrl: "./dataset-edit-form.component.html",
  styleUrls: ["./dataset-edit-form.component.scss"],
  encapsulation: ViewEncapsulation.None
})
export class DatasetEditFormComponent implements OnChanges {
  @Input() controls: FormGroup;
  @Input() licenseEntry: LicenseEntryType;
  @Input() updateCounter: number = 0;
  @Input() isNomination: boolean = false;
  @Input() includeOtherLicense: boolean = false;
  @Output() onSubmit = new EventEmitter<DatasetEditFormSubmitData>();

  public isDatasetUrlDistinct: boolean;

  private lastDatasetUrl: string;
  private lastUpdateCounter: number = 0;

  constructor(
    private modal: ModalDialogService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if(changes.updateCounter) {
      if(this.updateCounter !== this.lastUpdateCounter) {
        // Initialize the form
        const areDistinct = this.datasetUrl.value !== this.projectUrl.value;
        this.isDatasetUrlDistinct = areDistinct;
        this.datasetUrlEnabled = areDistinct;
      }
    }
  }

  get formTitle() {
    return this.isNomination ? "Nomination Details" : "Dataset Details";
  }

  get name() {
    return this.controls.get("name") as FormControl;
  }
  get description() {
    return this.controls.get("description") as FormControl;
  }
  get datasetUrl() {
    return this.controls.get("datasetUrl") as FormControl;
  }
  get projectUrl() {
    return this.controls.get("projectUrl") as FormControl;
  }
  get published() {
    return this.controls.get("published") as FormControl;
  }
  get version() {
    return this.controls.get("version") as FormControl;
  }
  get domain() {
    return this.controls.get("domain") as FormControl;
  }
  get license() {
    return this.controls.get("license") as FormControl;
  }
  get isFeatured() {
    return this.controls.get("isFeatured") as FormControl;
  }
  get tags() {
    return this.controls.get("tags") as FormControl;
  }
  get digitalObjectIdentifier() {
    return this.controls.get("digitalObjectIdentifier") as FormControl;
  }
  get contactName() {
    return this.controls.get("contactName") as FormControl;
  }
  get contactInfo() {
    return this.controls.get("contactInfo") as FormControl;
  }

  onDatasetUrlDistinctChanged() {
    if (this.isDatasetUrlDistinct) {
      this.lastDatasetUrl = this.datasetUrl.value;
      this.datasetUrl.setValue(this.projectUrl.value);
      this.datasetUrl.clearValidators();
      this.datasetUrlEnabled = false;
      this.isDatasetUrlDistinct = false;
    } else {
      this.datasetUrl.setValue(this.lastDatasetUrl);
      this.datasetUrlEnabled = true;
      this.isDatasetUrlDistinct = true;
    }
  }

  onProjectUrlChanged() {
    if (!this.isDatasetUrlDistinct) {
      this.datasetUrl.setValue(this.projectUrl.value);
    }
  }

  get datasetUrlEnabled() {
    return this.datasetUrl.enabled;
  }
  set datasetUrlEnabled(isEnabled: boolean) {
    if (isEnabled) {
      this.datasetUrl.enable();
    } else {
      this.datasetUrl.disable();
    }
  }

  onEnterLicense() {
    this.modal.create({
      title: "License",
      component: DatasetLicenseDialogComponent,
      params: {
        entry: this.licenseEntry
      },
      onClose: (result?: LicenseEntryType) => {
        if (result) {
          this.licenseEntry = result;
          const control = this.license;
          control.setErrors(null);
          control.markAsDirty();
        }
      }
    });
  }

  get isFormValid() {
    return this.controls.valid;
  }

  submitDatasetForm() {
    const normalize = (value: any) => {
      if (typeof value === "string") {
        value = value.trim();
      }
      return value ? value : undefined;
    };

    const dataset: DatasetFields = {
      name: normalize(this.name.value),
      description: normalize(this.description.value),
      projectUrl: normalize(this.projectUrl.value),
      datasetUrl: normalize(this.datasetUrl.value),
      published: normalize(this.published.value),
      domainId: normalize(
        this.domain.value && this.domain.value.id
      ),
      domain: normalize(
        this.domain.value && this.domain.value.name
      ),
      licenseId: normalize(
        this.license.value && this.license.value.id
      ),
      licenseName: normalize(
        this.license.value && this.license.value.name
      ),
      isFeatured: this.isNomination ? false : Boolean(this.isFeatured.value),
      version: normalize(this.version.value),
      digitalObjectIdentifier: normalize(
        this.digitalObjectIdentifier.value
      ),
      tags: normalize(this.tags.value) || [],
      contactName: normalize(this.contactName.value),
      contactInfo: normalize(this.contactInfo.value)
    };
    this.onSubmit.emit([dataset, this.licenseEntry]);
  }
}
