// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {Component, Input} from '@angular/core';
import {AbstractControl} from '@angular/forms';

@Component({
  selector: 'app-errmsg',
  templateUrl: './errmsg.component.html',
  styles: [`
.help-block {
  color: #a94442;
  margin-left: 15px;
}
  `]
})
export class ErrMsgComponent {
  @Input() inputControl: AbstractControl;
  @Input() errorCode: string;

  get shouldShowError() {
    const {inputControl, errorCode} = this;
    return inputControl && inputControl.hasError(errorCode);
  }
}
