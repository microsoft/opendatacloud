import { HttpErrorResponse } from "@angular/common/http";
import { Component, OnInit, OnDestroy } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import {
  DatasetEdit,
  isEmptyValue,
  LicenseEntryType,
  LicenseContentType,
  DatasetEditStatus
} from "odr-ui-shared";
import { EMPTY, merge, Observable, Subject, of, Subscription } from "rxjs";
import {
  catchError,
  filter,
  map,
  switchMap,
  tap,
  combineLatest,
  mergeMap,
  delay
} from "rxjs/operators";
import { AuthService, OdrService } from "../../shared/services";
import { SubmissionStatus } from "../../shared/types/submission.type";
import { FormGroup, FormControl, Validators } from "@angular/forms";
import { isServerErrors, ServerError } from "../../shared/types/server-errors";
import { DatasetEditFormSubmitData } from "./dataset-edit-form.component";

export enum LoadStatus {
  Loading = 0,
  Loaded = 1,
  Saving = 2,
  Error = -1
}

@Component({
  selector: "app-dataset-edit-loader",
  template: `
    <ng-content></ng-content>
  `
})
export class DatasetEditLoaderComponent implements OnInit, OnDestroy {
  controls: FormGroup;
  licenseEntry: LicenseEntryType = {
    contentType: LicenseContentType.Unknown
  };
  status: LoadStatus = LoadStatus.Loading;
  datasetId: string = '';
  editStatus: DatasetEditStatus = DatasetEditStatus.Unmodified;
  updateCounter: number = 0;

  private updateSubject = new Subject<DatasetEdit>();
  private refreshSubject = new Subject<DatasetEdit>();
  private datasetObservable: Observable<DatasetEdit>;
  private subscription: Subscription | undefined;

  constructor(
    private route: ActivatedRoute,
    private authService: AuthService,
    private odrService: OdrService
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

    const patchValues = () => (source: Observable<DatasetEdit>) => {
      return source.pipe(
        tap(dataset => {
          const userDetails = this.authService.getAuthenticatedUserDetails();
          this.controls.patchValue({
            name: dataset.name,
            description: dataset.description,
            datasetUrl: dataset.datasetUrl,
            projectUrl: dataset.projectUrl,
            published: isEmptyValue(dataset.published)
              ? null
              : new Date(dataset.published),
            version: dataset.version,
            domain: {
              id: dataset.domainId,
              name: dataset.domain
            },
            license: {
              id: dataset.licenseId,
              name: dataset.licenseName
            },
            tags: dataset.tags || [],
            digitalObjectIdentifier: dataset.digitalObjectIdentifier,
            contactName: isEmptyValue(dataset.contactName)
              ? userDetails.name
              : dataset.contactName,
            contactInfo: isEmptyValue(dataset.contactInfo)
              ? (userDetails.emails && userDetails.emails[0]) || ""
              : dataset.contactInfo
          });
          this.datasetId = dataset.id;
          this.editStatus = dataset.editStatus;
          this.updateCounter += 1;
        })
      );
    };

    const authStream = this.authService
      .getAuthenticationStatus()
      .pipe(filter(authenticated => authenticated));

    const fetchData = () => (source: Observable<string>) => {
      return source.pipe(
        tap(() => {
          this.status = LoadStatus.Loading;
        }),
        switchMap(datasetId => this.odrService.getDatasetEditById(datasetId)),
        patchValues(),
        tap(() => {
          this.status = LoadStatus.Loaded;
        }),
        catchError(err => {
          this.status = LoadStatus.Error;
          if (!(err instanceof HttpErrorResponse && err.status === 404)) {
            console.error(err);
          }
          return EMPTY;
        })
      );
    };

    const routeStream = this.route.params.pipe(
      combineLatest(authStream),
      map(([params]) => params["id"]),
      filter(id => !isEmptyValue(id)),
      fetchData()
    );

    const updateStream = this.updateSubject.asObservable().pipe(
      tap(() => {
        this.status = LoadStatus.Saving;
        this.controls.reset();
      }),
      this.odrService.submitDatasetEdit(),
      tap(submission => {
        this.status = LoadStatus.Loaded;
        if (submission.status === SubmissionStatus.error) {
          const { error } = submission;
          if (error && !error.isValid) {
            this.addServerErrors(error.errors);
          }
        }
      }),
      mergeMap(submssion => {
        return submssion.status === SubmissionStatus.success
          ? of(submssion.result as DatasetEdit)
          : EMPTY;
      }),
      patchValues(),
      catchError(err => {
        this.status = LoadStatus.Error;
        console.error(err);
        return EMPTY;
      })
    );

    const refreshStream = this.refreshSubject.asObservable().pipe(patchValues());

    this.datasetObservable = merge(routeStream, updateStream, refreshStream);
  }

  ngOnInit(): void {
    this.subscription = this.datasetObservable.subscribe();
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
      this.subscription = undefined;
    }
  }

  get isLoading() {
    return this.status === LoadStatus.Loading;
  }
  get isSaving() {
    return this.status === LoadStatus.Saving;
  }
  get isLoaded() {
    return this.status === LoadStatus.Loaded;
  }
  get hasLoadError() {
    return this.status === LoadStatus.Error;
  }

  refreshDataset(dataset: DatasetEdit) {
    this.refreshSubject.next(dataset);
  }

  updateDataset([dataset, otherLicense]: DatasetEditFormSubmitData) {
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

    let licenseDetails: Partial<DatasetEdit> = {};
    if (otherLicense.contentType === LicenseContentType.HtmlText) {
      licenseDetails = {
        otherLicenseName: otherLicense.name,
        otherLicenseAdditionalInfoUrl: otherLicense.additionalInfoUrl,
        otherLicenseContentHtml: otherLicense.contentHtml
      };
    } else if (otherLicense.contentType === LicenseContentType.InputFile) {
      licenseDetails = {
        otherLicenseName: otherLicense.name,
        otherLicenseAdditionalInfoUrl: otherLicense.additionalInfoUrl,
        otherLicenseFile: otherLicense.fileInput,
        otherLicenseFileName: otherLicense.fileName
      };
    } else {
      licenseDetails = {
        licenseId: licenseId,
        licenseName: licenseName
      };
    }

    const update: DatasetEdit = {
      id: this.datasetId,
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
      editStatus: DatasetEditStatus.DetailsModified,
      ...licenseDetails
    };

    this.updateSubject.next(update);
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
}
