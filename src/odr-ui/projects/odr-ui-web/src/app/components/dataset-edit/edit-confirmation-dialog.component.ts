// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, Input, OnInit } from "@angular/core";
import { DatasetEditStatus, ModalDialogService } from "odr-ui-shared";
import { OdrService } from "../../shared/services";

export class EditConfirmationDialogConfig {
  datasetId: string;
  editStatus: DatasetEditStatus;
  title: string;
  content: string;
  confirmText: string;
  cancelText: string;
  workingText: string;
  doWork: () => Promise<void>;
}

@Component({
  selector: "app-edit-confirmation-dialog",
  styleUrls: ["../../shared/styles/dialog.component.css"],
  templateUrl: "./edit-confirmation-dialog.component.html"
})
export class EditConfirmationDialogComponent implements OnInit {
  @Input() config: EditConfirmationDialogConfig;

  working: boolean = false;

  constructor(private modal: ModalDialogService) {}

  ngOnInit(): void {
    if (!this.config) {
      console.log("no config");
      return;
    }
  }

  get title() {
    return this.working ? this.config.workingText : this.config.title;
  }

  public onConfirm(): void {
    this.working = true;
    this.config.doWork().then(() => {
      this.modal.close();
    });
  }

  public onCancel(): void {
    this.modal.close();
  }
}
