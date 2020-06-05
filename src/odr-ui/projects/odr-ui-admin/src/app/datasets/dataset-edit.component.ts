// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { HttpErrorResponse } from "@angular/common/http";
import { Component } from "@angular/core";
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { ToasterService } from "angular2-toaster";
import {
  isEmptyValue,
  LicenseContentType,
  LicenseEntryType
} from "odr-ui-shared";
import { Observable, of, merge, Subject, concat, EMPTY } from "rxjs";
import {
  catchError,
  map,
  startWith,
  switchMap,
  tap,
  mergeMap
} from "rxjs/operators";
import { Dataset } from "../models/dataset.type";
import { DatasetsService } from "../services/datasets.service";
import { DatasetEditFormSubmitData } from "./dataset-edit-form.component";
import { isServerErrors, ServerError } from "../services/server-errors";

@Component({
  selector: "app-dataset-edit",
  templateUrl: "./dataset-edit.component.html",
  styleUrls: ["./dataset-edit.component.scss"]
})
export class DatasetEditComponent {
  controls: FormGroup;
  licenseEntry: LicenseEntryType = {
    contentType: LicenseContentType.Unknown
  };
  updateCounter: number = 0;
  loading: Observable<boolean>;
  current: {
    datasetId?: string;
    datasetName?: string;
  } = {};

  private updateSubject = new Subject<Dataset>();

  constructor(
    private datasetsService: DatasetsService,
    private toasterService: ToasterService,
    private route: ActivatedRoute,
    private router: Router
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
      isFeatured: new FormControl(false, []),
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

    const updateForm = () => (source: Observable<Dataset>) => {
      return source.pipe(
        tap(dataset => {
          const contactName = (dataset.datasetOwners || [])
            .map(({ name }) => name)
            .join(";");
          const contactInfo = (dataset.datasetOwners || [])
            .map(({ email }) => email)
            .join(";");
          this.current = {
            datasetId: dataset.id,
            datasetName: dataset.name
          };
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
            isFeatured: Boolean(dataset.isFeatured),
            contactName,
            contactInfo
          });
          this.updateCounter += 1;
        })
      );
    };

    const fetchDataset = () => (source: Observable<string>) => {
      return source.pipe(
        switchMap(datasetId => this.datasetsService.getDataset(datasetId)),
        updateForm(),
        catchError(err => {
          if (err instanceof HttpErrorResponse) {
            if (err.status === 404) {
              this.toasterService.pop(
                "error",
                "Not Found",
                "Dataset was not found."
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
      map(({ datasetId }) => datasetId),
      fetchDataset()
    );

    const updateStream = this.updateSubject.asObservable().pipe(
      switchMap(dataset =>
        this.datasetsService.updateDataset(dataset).pipe(map(() => dataset.id))
      ),
      tap(() => {
        this.toasterService.pop("info", "Success", "Dataset saved.");
        this.controls.reset();
      }),
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
      fetchDataset()
    );

    this.loading = merge(routeStream, updateStream).pipe(
      startWith(false),
      map(() => true)
    );
  }

  get canUpdate() {
    return !this.controls.pristine && this.controls.valid;
  }

  onSubmitDataset([dataset]: DatasetEditFormSubmitData) {
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
      isFeatured,
      tags,
      digitalObjectIdentifier,
      contactName,
      contactInfo
    } = dataset;

    const contactNames = contactName.split(";");
    const contactEmails = contactInfo.split(";");
    const datasetOwners = contactEmails.reduce((list, email, idx) => {
      if (!isEmptyValue(email)) {
        const name = contactNames[idx] || undefined;
        list.push({
          name,
          email
        });
      }
      return list;
    }, []);

    const update: Dataset = {
      id: this.current.datasetId,
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
      isFeatured,
      datasetOwners,
      digitalObjectIdentifier
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
    }
  }

  onDeleteDataset() {
    const msg = `Do you wish to permanently delete the "${this.current.datasetName}" dataset and remove it from the system?`;
    if (confirm(msg)) {
      this.datasetsService
        .deleteDataset(this.current.datasetId)
        .pipe(
          catchError(err => {
            console.error(err);
            return of(null);
          }),
          tap(() => {
            this.router.navigate(["datasets"]);
          })
        )
        .subscribe();
    }
  }
}
