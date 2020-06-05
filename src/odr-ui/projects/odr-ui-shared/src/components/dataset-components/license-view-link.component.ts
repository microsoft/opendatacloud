// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, Inject, Input } from "@angular/core";

@Component({
  selector: "app-license-view-link",
  templateUrl: "./license-view-link.component.html",
  styles: [
    `
      :host {
        display: inline-block;
      }
      a {
        padding: 0.1rem;
      }
      svg {
        width: 1.6rem;
        height: 0.8rem;
      }
    `
  ]
})
export class LicenseViewLinkComponent {
  @Input() licenseId: string;

  constructor(
    @Inject("API_BASE_URL") private apiBaseUrl: string
  ) {}

  get licenseUrl() {
    return `${this.apiBaseUrl}licenses/${this.licenseId}/view`;
  }
}
