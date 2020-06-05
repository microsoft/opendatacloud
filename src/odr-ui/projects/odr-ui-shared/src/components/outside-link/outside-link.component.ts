// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-outside-link',
  templateUrl: './outside-link.component.html',
  styleUrls: ['./outside-link.component.scss']
})
export class OutsideLinkComponent {
  @Input() title: string;
  @Input() url: string;
  @Input() hideUnderline: boolean;

  get rootClasses(): string {
    return this.hideUnderline ? 'no-underline' : '';
  }
}
