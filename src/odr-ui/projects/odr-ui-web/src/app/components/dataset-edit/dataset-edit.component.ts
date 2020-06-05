// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, ViewChild } from "@angular/core";
import { DatasetEdit, DatasetEditStatus } from "odr-ui-shared";
import {
  DatasetEditLoaderComponent,
  LoadStatus
} from "./dataset-edit-loader.component";
import { DatasetEditFormSubmitData } from "./dataset-edit-form.component";
import { ToasterService } from "angular2-toaster";

@Component({
  selector: "app-dataset-edit",
  templateUrl: "./dataset-edit.component.html",
  styleUrls: ["./dataset-edit.component.scss"]
})
export class DatasetEditComponent {
  @ViewChild("loader", { static: false })
  datasetLoader: DatasetEditLoaderComponent;

  constructor(private toasterService: ToasterService) {}

  get dataset(): DatasetEdit {
    return {} as DatasetEdit;
  }

  get isSubmitDisabled() {
    return (
      !this.datasetLoader ||
      !this.datasetLoader.controls.valid ||
      this.datasetLoader.controls.pristine
    );
  }

  get isImporting() {
    return this.datasetLoader && this.datasetLoader.editStatus === DatasetEditStatus.Importing;
  }

  onSubmitDataset(formData: DatasetEditFormSubmitData) {
    this.datasetLoader.updateDataset(formData);
    window.scrollTo({
      top: 0,
      left: 0,
      behavior: "smooth"
    });
    setTimeout(() => {
      this.toasterService.pop("success", "Success", "Saved pending dataset changes.");
    }, 250);
  }
}
