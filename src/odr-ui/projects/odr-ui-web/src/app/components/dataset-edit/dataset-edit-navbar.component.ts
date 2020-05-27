import { Component, Input, Output, EventEmitter } from "@angular/core";
import {
  DatasetEditStatus,
  ModalDialogService,
  DatasetEdit
} from "odr-ui-shared";
import { OdrService } from "../../shared/services";
import { Router } from "@angular/router";
import {
  EditConfirmationDialogConfig,
  EditConfirmationDialogComponent
} from "./edit-confirmation-dialog.component";
import { take, map } from "rxjs/operators";
import {
  StorageTokenDialogConfig,
  StorageTokenDialogComponent
} from "./storage-token-dialog.component";

@Component({
  selector: "app-dataset-edit-navbar",
  templateUrl: "./dataset-edit-navbar.component.html",
  styleUrls: ["./dataset-edit-navbar.component.scss"]
})
export class DatasetEditNavbarComponent {
  @Input() datasetId: string;
  @Input() editStatus: DatasetEditStatus;
  @Output() refreshDataset = new EventEmitter<DatasetEdit>();

  constructor(
    private odrService: OdrService,
    private modal: ModalDialogService,
    private router: Router
  ) {}

  get unmodified() {
    return this.editStatus === DatasetEditStatus.Unmodified;
  }

  get detailsModified() {
    return this.editStatus === DatasetEditStatus.DetailsModified;
  }

  get contentModified() {
    return this.editStatus === DatasetEditStatus.ContentsModified;
  }

  get isImporting() {
    return this.editStatus === DatasetEditStatus.Importing;
  }

  get allowModifyContents() {
    return (
      this.editStatus === DatasetEditStatus.Unmodified ||
      this.editStatus === DatasetEditStatus.DetailsModified
    );
  }

  get allowPublishChanges() {
    return (
      this.editStatus === DatasetEditStatus.DetailsModified ||
      this.editStatus === DatasetEditStatus.ContentsModified
    );
  }

  onModifyContent() {
    const title = "Modify Dataset Content?";
    const config: EditConfirmationDialogConfig = {
      datasetId: this.datasetId,
      editStatus: this.editStatus,
      title,
      content:
        "Do you wish to modify the contents of this dataset? (This will create a new container in which to store the updated dataset content.)",
      confirmText: "Modify Content",
      cancelText: "Cancel",
      workingText: "Creating storage ...",
      doWork: () => {
        return this.odrService
          .modifyDatasetContent(this.datasetId)
          .pipe(take(1))
          .toPromise()
          .then(dataset => {
            this.refreshDataset.emit(dataset);
          });
      }
    };
    this.modal.create({
      title,
      component: EditConfirmationDialogComponent,
      params: {
        config
      }
    });
  }

  onPublishChanges() {
    const isContentChange =
      this.editStatus === DatasetEditStatus.ContentsModified;
    const title = "Publish Changes?";
    const config: EditConfirmationDialogConfig = {
      datasetId: this.datasetId,
      editStatus: this.editStatus,
      title,
      content: isContentChange
        ? "Are you finished updating the dataset content? Are you sure you wish to publish the changes made to this dataset?"
        : "Are you sure you wish to publish the changes made to this dataset?",
      confirmText: "Publish",
      cancelText: "Cancel",
      workingText: "Publishing changes ...",
      doWork: () => {
        return this.odrService
          .publishDatasetEditChanges(this.datasetId)
          .pipe(take(1))
          .toPromise()
          .then(() => {
            setTimeout(() => {
              this.router.navigate(["/datasets", this.datasetId]);
            }, 150);
          });
      }
    };
    this.modal.create({
      title,
      component: EditConfirmationDialogComponent,
      params: {
        config
      }
    });
  }

  onAbandonChanges() {
    const title = "Abandon Changes?";
    const config: EditConfirmationDialogConfig = {
      datasetId: this.datasetId,
      editStatus: this.editStatus,
      title,
      content: "Do you wish to abandon any changes made to this dataset?",
      confirmText: "Abandon",
      cancelText: "Keep",
      workingText: "Abandoning changes ...",
      doWork: () => {
        return this.odrService
          .cancelDatasetEditChanges(this.datasetId)
          .pipe(take(1))
          .toPromise()
          .then(() => {
            setTimeout(() => {
              this.router.navigate(["/datasets", this.datasetId]);
            }, 150);
          });
      }
    };
    this.modal.create({
      title,
      component: EditConfirmationDialogComponent,
      params: {
        config
      }
    });
  }

  onGetUpdatedToken() {
    const title = "Updated Content";
    const config: StorageTokenDialogConfig = {
      title,
      content:
        "Use this SAS token to copy the updated contents of the dataset into the storage container.",
      getToken: () => {
        return this.odrService
          .getDatasetEditUpdatedContentToken(this.datasetId)
          .pipe(
            take(1),
            map(({ token }) => token)
          )
          .toPromise();
      }
    };
    this.modal.create({
      title,
      component: StorageTokenDialogComponent,
      params: {
        config
      }
    });
  }

  onGetOriginalToken() {
    const title = "Original Content";
    const config: StorageTokenDialogConfig = {
      title,
      content:
        "Use this read-only SAS token to retrieve the contents of the original dataset if needed.",
      getToken: () => {
        return this.odrService
          .getDatasetEditOriginalContentToken(this.datasetId)
          .pipe(
            take(1),
            map(({ token }) => token)
          )
          .toPromise();
      }
    };
    this.modal.create({
      title,
      component: StorageTokenDialogComponent,
      params: {
        config
      }
    });
  }
}
