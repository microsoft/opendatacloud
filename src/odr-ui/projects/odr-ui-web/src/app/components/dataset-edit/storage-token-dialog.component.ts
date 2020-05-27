import { Component, Input, OnInit } from "@angular/core";
import { ModalDialogService } from "odr-ui-shared";
import { ToasterService } from "angular2-toaster";

export class StorageTokenDialogConfig {
  title: string;
  content: string;
  getToken: () => Promise<string>;
}

@Component({
  selector: "app-storage-token-dialog",
  styleUrls: [
    "../../shared/styles/dialog.component.css",
    "./storage-token-dialog.component.scss"
  ],
  templateUrl: "./storage-token-dialog.component.html"
})
export class StorageTokenDialogComponent implements OnInit {
  @Input() config: StorageTokenDialogConfig;

  datasetToken: string = "";

  constructor(
    private modal: ModalDialogService,
    private toasterService: ToasterService
  ) {}

  ngOnInit(): void {
    if (!this.config) {
      console.error("No config.");
      return;
    }

    this.config.getToken().then(token => {
      this.datasetToken = token;
    });
  }

  public onCopyToken(): void {
    this.toasterService.pop("success", "Success", "Copied token to clipboard.");
    this.modal.close();
  }

  public onClose(): void {
    this.modal.close();
  }
}
