import { HttpErrorResponse } from "@angular/common/http";
import { Component, ViewChild } from "@angular/core";
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { ToasterService } from "angular2-toaster";
import {
  DatasetNomination,
  isEmptyValue,
  LicenseContentType,
  LicenseEntryType,
  NominationLicenseType
} from "odr-ui-shared";
import { EMPTY, merge, Observable, of, Subject } from "rxjs";
import {
  catchError,
  filter,
  map,
  mergeMap,
  switchMap,
  tap
} from "rxjs/operators";
import {
  DatasetEditFormComponent,
  DatasetEditFormSubmitData
} from "../datasets/dataset-edit-form.component";
import { DatasetStorage } from "../models";
import { NominationStatus } from "../models/nomination-status.enum";
import { NominationsService } from "../services";
import { isServerErrors, ServerError } from "../services/server-errors";

type NominationOperation = {
  (): Observable<any>;
};

@Component({
  selector: "app-nomination-edit",
  templateUrl: "./nomination-edit.component.html",
  styleUrls: ["./nomination-edit.component.scss"]
})
export class NominationEditComponent {
  @ViewChild("editForm", { static: false }) editForm: DatasetEditFormComponent;

  controls: FormGroup;
  licenseEntry: LicenseEntryType = {
    contentType: LicenseContentType.Unknown
  };
  nomination: Observable<DatasetNomination>;
  datasetStorage: DatasetStorage = null;
  updateCounter: number = 0;

  private operationSubject = new Subject<NominationOperation>();
  private get nominationId() {
    return this.currentNomination.id;
  }
  private currentNomination: DatasetNomination;

  constructor(
    private nominationsService: NominationsService,
    private router: Router,
    private route: ActivatedRoute,
    private toasterService: ToasterService
  ) {
    this.controls = new FormGroup({
      name: new FormControl("", [
        Validators.required,
        Validators.maxLength(128)
      ]),
      description: new FormControl("", [
        Validators.required,
        Validators.maxLength(5000)
      ]),
      datasetUrl: new FormControl("", [
        Validators.maxLength(1024),
        Validators.required
      ]),
      projectUrl: new FormControl("", [
        Validators.required,
        Validators.maxLength(1024)
      ]),
      published: new FormControl(null, [Validators.required]),
      version: new FormControl("", [
        Validators.required,
        Validators.maxLength(50)
      ]),
      domain: new FormControl(null, [Validators.required]),
      license: new FormControl(null, [Validators.required]),
      tags: new FormControl(),
      digitalObjectIdentifier: new FormControl("", [Validators.maxLength(255)]),
      contactName: new FormControl("", [
        Validators.required,
        Validators.maxLength(128)
      ]),
      contactInfo: new FormControl("", [
        Validators.required,
        Validators.maxLength(2000)
      ])
    });

    const updateForm = () => (source: Observable<DatasetNomination>) => {
      return source.pipe(
        tap(nomination => {
          this.currentNomination = nomination;
          this.controls.patchValue({
            name: nomination.name,
            description: nomination.description,
            datasetUrl: nomination.datasetUrl,
            projectUrl: nomination.projectUrl,
            published: isEmptyValue(nomination.published)
              ? null
              : new Date(nomination.published),
            version: nomination.version,
            domain: {
              id: nomination.domainId,
              name: nomination.domain
            },
            license: {
              id: nomination.licenseId,
              name: nomination.licenseName
            },
            tags: nomination.tags || [],
            digitalObjectIdentifier: nomination.digitalObjectIdentifier,
            contactName: nomination.contactName,
            contactInfo: nomination.contactInfo
          });
          this.controls.markAsPristine();
          this.controls.markAsUntouched();
          if (!this.isPendingApproval(nomination)) {
            this.controls.disable();
          }
          this.updateCounter += 1;
        })
      );
    };

    const fetchNomination = () => (source: Observable<string>) => {
      return source.pipe(
        switchMap(nominationId => {
          if(nominationId === "create") {

            // setTimeout(() => {
            //   this.controls.patchValue({
            //     name: "Test Dataset",
            //     published: new Date("2018-12-31"),
            //     description: "Dataset description text.",
            //     projectUrl: "http://microsoft.com/research/one",
            //     datasetUrl: "http://microsoft.com/research/two",
            //     domain: {
            //       id: "COMPUTER SCIENCE",
            //       name: "computer science"
            //     },
            //     version: "1.0",
            //     digitalObjectIdentifier: "/some/id",
            //     contactName: "Test User",
            //     contactInfo: "dfbaskin@outlook.com"
            //   });
            // }, 1000);

            return of(DatasetNomination.createEmptyDatasetNomination());
          } else {
            return this.nominationsService.getDatasetNomination(nominationId)
          }
        }),
        updateForm(),
        mergeMap(nomination => {
          const { nominationStatus, id } = nomination;
          if (nominationStatus === NominationStatus.Approved) {
            return this.nominationsService.getDatasetStorage(id).pipe(
              tap(storage => {
                this.datasetStorage = storage;
              }),
              mergeMap(() => of(nomination))
            );
          } else {
            return of(nomination);
          }
        }),
        catchError(err => {
          if (err instanceof HttpErrorResponse) {
            if (err.status === 404) {
              this.toasterService.pop(
                "error",
                "Not Found",
                "Dataset nomination was not found."
              );
            } else {
              console.error(`${err.status} - ${err.statusText}`);
              this.toasterService.pop("error", "Error", err.statusText);
            }
          } else {
            console.error(err);
          }
          return EMPTY;
        })
      );
    };

    const routeStream = this.route.params.pipe(
      map(({ nominationId }) => nominationId),
      fetchNomination()
    );

    const refreshStream = this.operationSubject.asObservable().pipe(
      switchMap(operation => operation().pipe(map(() => this.nominationId))),
      catchError(err => {
        if (err instanceof HttpErrorResponse) {
          if (err.status === 400) {
            if (err.error && !Boolean(err.error.isValid) && err.error.errors) {
              this.addServerErrors(err.error.errors);
              return EMPTY;
            }
          }
          console.error(`${err.status} - ${err.statusText}`);
          this.toasterService.pop("error", "Error", err.statusText);
        } else {
          console.error(err);
        }
        return EMPTY;
      }),
      filter(id => Boolean(id)),
      fetchNomination()
    );

    const assetTokensStream = this.nominationsService
      .getAssetsAuthToken()
      .pipe(mergeMap(() => EMPTY));

    this.nomination = merge(routeStream, refreshStream, assetTokensStream);
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

  isNewNomination(nomination: DatasetNomination) {
    return nomination.id === "";
  }

  isPendingApproval(nomination: DatasetNomination) {
    return (
      !nomination.nominationStatus ||
      nomination.nominationStatus === NominationStatus.PendingApproval
    );
  }

  canUpdateCallback = () => {
    return this.controls.touched && this.controls.valid;
  };

  canApproveCallback = () => {
    if (this.controls.valid) {
      const formModel = this.controls.value;

      //  validate fields only required when approving
      return (
        formModel.published &&
        formModel.domain.id &&
        formModel.license &&
        formModel.version
      );
    }
    return false;
  };

  private nextUpdateOperation: (
    update: DatasetNomination
  ) => NominationOperation = null;

  onSubmit([dataset, otherLicense]: DatasetEditFormSubmitData) {
    // console.log(JSON.stringify({dataset, otherLicense}, null, 2));
    const {
      name,
      description,
      projectUrl,
      datasetUrl,
      published,
      version,
      domainId,
      domain,
      licenseId,
      licenseName,
      tags,
      digitalObjectIdentifier,
      contactName,
      contactInfo
    } = dataset;

    let licenseDetails: Partial<DatasetNomination> = {};
    if (otherLicense.contentType === LicenseContentType.HtmlText) {
      licenseDetails = {
        otherLicenseName: otherLicense.name,
        otherLicenseAdditionalInfoUrl: otherLicense.additionalInfoUrl,
        otherLicenseContentHtml: otherLicense.contentHtml,
        nominationLicenseType: NominationLicenseType.HtmlText
      };
    } else if (otherLicense.contentType === LicenseContentType.InputFile) {
      licenseDetails = {
        otherLicenseName: otherLicense.name,
        otherLicenseAdditionalInfoUrl: otherLicense.additionalInfoUrl,
        otherLicenseFile: otherLicense.fileInput,
        otherLicenseFileName: otherLicense.fileName,
        nominationLicenseType: NominationLicenseType.InputFile
      };
    } else {
      licenseDetails = {
        licenseId: licenseId,
        licenseName: licenseName,
        nominationLicenseType: licenseId
          ? NominationLicenseType.Standard
          : NominationLicenseType.Unknown
      };
    }

    const update: DatasetNomination = {
      ...this.currentNomination,
      name,
      description,
      projectUrl,
      datasetUrl,
      tags: tags,
      published,
      version,
      domain,
      domainId,
      licenseId,
      licenseName,
      contactName,
      contactInfo,
      digitalObjectIdentifier,
      ...licenseDetails
    };

    if (this.nextUpdateOperation) {
      this.operationSubject.next(this.nextUpdateOperation(update));
      this.nextUpdateOperation = null;
    }
  }

  addServerErrors(err?: any) {
    if (isServerErrors(err)) {
      const serverErrors: ServerError[] = err;
      Object.entries(this.controls.controls)
        .map(([name, control]) => {
          const entries = serverErrors.filter(({ propertyName }) => {
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

      this.licenseEntry.errors = serverErrors.reduce(
        (list, { propertyName, errorCode, errorMessage }) => {
          if (propertyName.toLowerCase() === "licenseurl") {
            list.push({
              propertyName,
              errorCode,
              errorMessage
            });
          }
          return list;
        },
        []
      );

      if (this.licenseEntry.errors.length > 0) {
        const control = this.controls.get("license");
        control.setErrors({
          licenseErrors: true
        });
        control.markAsDirty();
      }
    }
  }

  onUpdateNomination() {
    this.nextUpdateOperation = (update: DatasetNomination) => {
      return () => {
        return this.nominationsService.updateDatasetNomination(update).pipe(
          tap(() => {
            this.toasterService.pop("info", "Success", "Nomination saved.");
          })
        );
      };
    };
    this.editForm.submitDatasetForm();
  }

  onCreateNomination() {
    this.nextUpdateOperation = (update: DatasetNomination) => {
      return () => {
        return this.nominationsService.createDatasetNomination(update).pipe(
          tap((nominationId: string) => {
            this.toasterService.pop("info", "Success", "Nomination created.");
            this.router.navigate(['nominations', nominationId]);
          })
        );
      };
    };
    this.editForm.submitDatasetForm();
  }

  onApproveNomination() {
    this.nextUpdateOperation = (update: DatasetNomination) => {
      return () => {
        return this.nominationsService.approveDatasetNomination(update).pipe(
          tap(() => {
            this.toasterService.pop("info", "Success", "Nomination approved.");
          })
        );
      };
    };
    this.editForm.submitDatasetForm();
  }

  onRejectNomination() {
    this.operationSubject.next(() => {
      return this.nominationsService
        .rejectDatasetNomination(this.currentNomination)
        .pipe(
          tap(() => {
            this.toasterService.pop(
              "info",
              "Success",
              "Nomination was rejected."
            );
          })
        );
    });
  }

  onCreateStorageForNomination() {
    this.operationSubject.next(() => {
      return this.nominationsService
        .createDatasetStorage(this.datasetStorage)
        .pipe(
          tap(() => {
            this.toasterService.pop("info", "Success", "Storage was created.");
          })
        );
    });
  }

  onImportDatasetFromStorage() {
    this.operationSubject.next(() => {
      return this.nominationsService
        .importDatasetFromStorage(this.currentNomination)
        .pipe(
          tap(() => {
            this.toasterService.pop(
              "info",
              "Success",
              "Import process was queued."
            );
          })
        );
    });
  }
}
