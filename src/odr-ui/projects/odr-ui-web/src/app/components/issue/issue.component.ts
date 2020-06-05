// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable, Subject } from 'rxjs';
import { SubmissionStatus, Submission } from '../../shared/types/submission.type';
import { CompleterService } from 'ng2-completer';
import { DatasetIssue, Dataset } from '../../shared/types';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { take, switchMap } from 'rxjs/operators';

import {
  AuthService,
  DatasetService,
  OdrService
} from '../../shared/services';

interface SelectedDataset {
  id: string;
  name: string;
}

@Component({
  selector: 'app-issue',
  templateUrl: './issue.component.html',
  styleUrls: ['./issue.component.scss']
})
export class IssueComponent implements OnInit {

  private issueSubject: Subject<DatasetIssue>;
  submissionStatus = SubmissionStatus;
  submission: Observable<Submission>;

  isAuthenticated: Observable<boolean>;

  issueForm: FormGroup;

  datasetDataService: any;
  selectedDataset: SelectedDataset = null;

  constructor(
    private odrService: OdrService,
    private authService: AuthService,
    private datasetService: DatasetService,
    private completerService: CompleterService,
    private route: ActivatedRoute) {

    this.isAuthenticated = this.authService.getAuthenticationStatus();

    const userDetails = this.authService.getAuthenticatedUserDetails();

    this.issueForm = new FormGroup({
      datasetName: new FormControl(
        '',
        [Validators.required, Validators.maxLength(256)]
      ),
      issueDesc: new FormControl(
        '',
        [Validators.required, Validators.maxLength(1024)]
      ),
      submitterName: new FormControl(
        userDetails.name,
        [Validators.maxLength(128)]
      ),
      contactInfo: new FormControl(
        userDetails.emails && userDetails.emails[0],
        [Validators.maxLength(200)]
      ),
    });

    this.issueSubject = new Subject();
    this.submission = odrService.submitDatasetIssue(this.issueSubject.asObservable());
    this.datasetDataService = completerService.remote(this.odrService.nameSearchUrl, 'name', 'name');
  }

  get datasetName() { return this.issueForm.get('datasetName'); }
  get issueDesc() { return this.issueForm.get('issueDesc'); }
  get submitterName() { return this.issueForm.get('submitterName'); }
  get contactInfo() { return this.issueForm.get('contactInfo'); }

  datasetNameErrors(errorCode: string) { return this.datasetName.getError(errorCode) || {}; }
  issueDescErrors(errorCode: string) { return this.issueDesc.getError(errorCode) || {}; }
  submitterNameErrors(errorCode: string) { return this.submitterName.getError(errorCode) || {}; }
  contactInfoErrors(errorCode: string) { return this.contactInfo.getError(errorCode) || {}; }

  ngOnInit(): void {
    // Get Dataset
    this.route.params
      .pipe(
        take(1),
        switchMap((params: Params) => {
          return this.datasetService.getDetailDataset(params['id']);
        })
      )
      .subscribe((dataset: Dataset) => {
        if (!dataset) {
          return;
        }

        this.selectedDataset = {
            id: dataset.id,
            name: dataset.name
          };

        this.onBlurDataset();

      });
  }


  onDatasetNameChanged(evt) {
    this.selectedDataset = evt ? evt.originalObject : null;
  }

  onBlurDataset() {
    this.datasetName.setValue(this.selectedDataset ? this.selectedDataset.name : '');
  }

  submitIssue() {
    const {
      issueDesc,
      submitterName,
      contactInfo
    } = this.issueForm.value;
    const issue: DatasetIssue = {
      name: this.selectedDataset.name,
      datasetId: this.selectedDataset.id,
      description: issueDesc,
      contactName: submitterName || undefined,
      contactInfo: contactInfo || undefined,
    };
    this.issueSubject.next(issue);
  }
}
