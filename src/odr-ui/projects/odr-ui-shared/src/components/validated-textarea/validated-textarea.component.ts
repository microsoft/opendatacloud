// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {
  AfterContentInit, Component, ContentChildren, Input, QueryList
} from '@angular/core';
import {AbstractControl} from '@angular/forms';
import {ErrMsgComponent} from '../errmsg/errmsg.component';

@Component({
  selector: 'app-validated-textarea',
  templateUrl: './validated-textarea.component.html',
  styleUrls: ['./validated-textarea.component.scss']
})
export class ValidatedTextareaComponent implements AfterContentInit {

  @ContentChildren(ErrMsgComponent, {descendants: true}) errorMessages: QueryList<ErrMsgComponent>;

  @Input() inputControl: AbstractControl;
  @Input() size: number;
  @Input() labelText: string;
  @Input() required: boolean;

  get formGroup(): AbstractControl {
    return this.inputControl.parent;
  }

  ngAfterContentInit() {
    this.errorMessages.forEach((c, idx) => {
      c.inputControl = this.inputControl;
    });
  }

  get sizeClass() {
    return this.size ? `col-lg-${this.size}` : 'col-lg-12';
  }

  isErrorVisible(inputControl: AbstractControl)  {
    return !inputControl.pristine && !inputControl.valid;
  }

  get rootClasses() {
    const {sizeClass, inputControl} = this;
    return {
      [sizeClass]: true,
      'has-error': this.isErrorVisible(inputControl)
    };
  }
}
