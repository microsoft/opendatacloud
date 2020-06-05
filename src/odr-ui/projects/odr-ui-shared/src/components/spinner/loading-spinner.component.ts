// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  template: `<div class="loader">{{loadingMessage}}</div>`,
  styleUrls: ['./loading-spinner.component.scss']
})
export class LoadingSpinnerComponent {
  @Input() message: string;

  get loadingMessage() {
    return this.message || "Loading ...";
  }
}
