// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


export enum SubmissionStatus {
  none,
  submitting,
  success,
  error
}

export interface Submission {
  status: SubmissionStatus;
  result?: any;
  error?: any;
}
