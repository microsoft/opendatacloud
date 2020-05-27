import { Component, OnDestroy, OnInit } from "@angular/core";
import { FormControl, FormGroup, Validators } from "@angular/forms";
import {
  DatasetNomination,
  LicenseContentType,
  LicenseEntryType,
  NominationLicenseType
} from "odr-ui-shared";
import { EMPTY, Observable, of, Subject, Subscription } from "rxjs";
import { mergeMap, tap } from "rxjs/operators";
import { OdrService } from "../../shared/services";
import { AuthService } from "../../shared/services/auth.service";
import { isServerErrors, ServerError } from "../../shared/types/server-errors";
import { SubmissionStatus } from "../../shared/types/submission.type";
import { DatasetEditFormSubmitData } from "../dataset-edit/dataset-edit-form.component";

@Component({
  selector: "app-nominate",
  templateUrl: "./nominate.component.html"
})
export class NominateComponent implements OnInit, OnDestroy {
  isAuthenticated: Observable<boolean>;
  controls: FormGroup;
  licenseEntry: LicenseEntryType = {
    contentType: LicenseContentType.Unknown
  };
  updateCounter: number = 0;

  private nominationObservable: Observable<DatasetNomination>;
  private subscription: Subscription | undefined;
  private nominationSubject = new Subject<DatasetNomination>();
  private status: SubmissionStatus = SubmissionStatus.none;

  constructor(
    private authService: AuthService,
    private odrService: OdrService
  ) {
    const userDetails = this.authService.getAuthenticatedUserDetails();
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
      projectUrl: new FormControl("", [Validators.maxLength(1024)]),
      published: new FormControl(null, []),
      version: new FormControl("", [Validators.maxLength(50)]),
      domain: new FormControl(null, []),
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

    this.isAuthenticated = this.authService.getAuthenticationStatus();

    this.nominationObservable = this.nominationSubject.asObservable().pipe(
      this.odrService.submitDatasetNomination(),
      tap(submission => {
        this.status = submission.status;
        if (submission.status === SubmissionStatus.error) {
          const { error } = submission;
          if (error && !error.isValid) {
            this.addServerErrors(error.errors);
          }
        }
      }),
      mergeMap(submission => {
        return submission.status === SubmissionStatus.success
          ? of(submission.result as DatasetNomination)
          : EMPTY;
      })
    );
  }

  get isEditingForm() {
    return this.status !== SubmissionStatus.success;
  }

  get hasSubmittedForm() {
    return this.status === SubmissionStatus.success;
  }

  get isSubmittingForm() {
    return this.status === SubmissionStatus.submitting;
  }

  get hasSubmissionError() {
    return this.status === SubmissionStatus.error;
  }

  get isSubmitDisabled() {
    return this.isSubmittingForm || !this.controls.valid;
  }

  ngOnInit(): void {
    this.subscription = this.nominationObservable.subscribe();
    setTimeout(() => {

      const userDetails = this.authService.getAuthenticatedUserDetails();
      this.controls.patchValue({
        contactName: userDetails.name,
        contactInfo: (userDetails.emails && userDetails.emails[0]) || ""
      });

      // this.controls.patchValue({
      //   name: "Test Dataset",
      //   published: new Date("2018-12-31"),
      //   description: "Dataset description text.",
      //   projectUrl: "http://microsoft.com/research/one",
      //   datasetUrl: "http://microsoft.com/research/two",
      //   domain: {
      //     id: "COMPUTER SCIENCE",
      //     name: "computer science"
      //   },
      //   version: "1.0",
      //   digitalObjectIdentifier: "/some/id"
      // });

      this.updateCounter += 1;
    });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
      this.subscription = undefined;
    }
  }

  submitNomination([dataset, otherLicense]: DatasetEditFormSubmitData) {
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

    const nomination: DatasetNomination = {
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

    this.nominationSubject.next(nomination);
  }

  addServerErrors(err?: any) {
    if (isServerErrors(err)) {
      const serverErrors: ServerError[] = err;
      Object.entries(this.controls)
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
}
