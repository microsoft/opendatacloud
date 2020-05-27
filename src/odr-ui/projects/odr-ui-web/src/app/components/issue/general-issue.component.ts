import { Component, NgZone } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable, Subject } from 'rxjs';
import { Submission, SubmissionStatus } from '../../shared/types/submission.type';

import { AuthService, OdrService } from '../../shared/services';
import { GeneralIssue } from '../../shared/types/general-issue.type';

const observeOnZone = <T>(zone: NgZone) => (source: Observable<T>) => new Observable<T>((subscriber) => {
  source.subscribe({
    next: (value) => zone.run(() => subscriber.next(value)),
    error: (err) => zone.run(() => subscriber.error(err)),
    complete: () => zone.run(() => subscriber.complete()),
  });
});

@Component({
  selector: 'app-general-issue',
  templateUrl: './general-issue.component.html',
  styleUrls: ['./general-issue.component.scss']
})
export class GeneralIssueComponent {

  private issueSubject: Subject<GeneralIssue>;
  submissionStatus = SubmissionStatus;
  submission: Observable<Submission>;

  isAuthenticated: Observable<boolean>;

  issueForm: FormGroup;

  constructor(
    private odrService: OdrService,
    private authService: AuthService,
    private ngZone: NgZone) {

    this.isAuthenticated = this.authService.getAuthenticationStatus();

    const userDetails = this.authService.getAuthenticatedUserDetails();

    this.issueForm = new FormGroup({
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
    this.submission = odrService
      .submitGeneralIssue(this.issueSubject.asObservable())
      .pipe(
        observeOnZone(ngZone)
      );
  }

  get issueDesc() { return this.issueForm.get('issueDesc'); }
  get submitterName() { return this.issueForm.get('submitterName'); }
  get contactInfo() { return this.issueForm.get('contactInfo'); }

  issueDescErrors(errorCode: string) { return this.issueDesc.getError(errorCode) || {}; }
  submitterNameErrors(errorCode: string) { return this.submitterName.getError(errorCode) || {}; }
  contactInfoErrors(errorCode: string) { return this.contactInfo.getError(errorCode) || {}; }

  submitIssue() {
    const {
      issueDesc,
      submitterName,
      contactInfo
    } = this.issueForm.value;
    const issue: GeneralIssue = {
      description: issueDesc,
      contactName: submitterName || undefined,
      contactInfo: contactInfo || undefined,
    };
    this.issueSubject.next(issue);
  }
}
