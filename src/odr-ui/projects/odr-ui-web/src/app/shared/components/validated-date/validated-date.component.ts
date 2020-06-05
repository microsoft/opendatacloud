// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {
    AfterContentInit, Component, ContentChildren, Input, QueryList
  } from '@angular/core';
  import {AbstractControl, FormGroup} from '@angular/forms';
  import {ErrMsgComponent} from 'odr-ui-shared';
  import {getInputControlName} from '../../utils/input-control-name';

  @Component({
    selector: 'app-validated-date',
    templateUrl: './validated-date.component.html',
    styleUrls: ['./validated-date.component.scss']
  })
  export class ValidatedDateComponent implements AfterContentInit {

    @ContentChildren(ErrMsgComponent, {descendants: true}) errorMessages: QueryList<ErrMsgComponent>;

    @Input() inputControl: AbstractControl;
    @Input() size: number;
    @Input() labelText: string;
    @Input() type: string;
    @Input() required: boolean;

    get form() {
      return this.inputControl.parent as FormGroup;
    }

    get name(): string {
      return getInputControlName(this.inputControl);
    }

    get inputType(): string {
      return this.type || 'text';
    }

    ngAfterContentInit() {
      this.errorMessages.forEach((c, idx) => {
        c.inputControl = this.inputControl;
      });
    }

    get sizeClass() {
      return this.size ? `col-lg-${this.size}` : 'col-lg-12';
    }

    get rootClasses() {
      const {sizeClass, inputControl} = this;
      return {
        [sizeClass]: true,
        'has-error': !inputControl.valid && !inputControl.pristine
      };
    }
  }
