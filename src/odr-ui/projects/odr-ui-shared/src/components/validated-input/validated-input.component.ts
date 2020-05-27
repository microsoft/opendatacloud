import {AfterContentInit, Component, ContentChildren, Input, QueryList} from '@angular/core';
import {AbstractControl} from '@angular/forms';
import {ErrMsgComponent} from '../errmsg/errmsg.component';

@Component({
  selector: 'app-validated-input',
  templateUrl: './validated-input.component.html',
  styleUrls: ['./validated-input.component.scss']
})
export class ValidatedInputComponent implements AfterContentInit {

  @ContentChildren(ErrMsgComponent, {descendants: true}) errorMessages: QueryList<ErrMsgComponent>;

  @Input() inputControl: AbstractControl;
  @Input() size: number;
  @Input() labelText: string;
  @Input() type: string;
  @Input() required: boolean;

  get formGroup(): AbstractControl {
    return this.inputControl.parent;
  }

  get inputType(): string {
    return this.type || 'text';
  }

  ngAfterContentInit() {
    this.errorMessages.forEach((c, idx) => {
      c.inputControl = this.inputControl;
    });
  }

  isErrorVisible(inputControl: AbstractControl)  {
    return !inputControl.pristine && !inputControl.valid;
  }

  get sizeClass() {
    return this.size ? `col-lg-${this.size}` : 'col-lg-12';
  }

  get rootClasses() {
    const {sizeClass, inputControl} = this;
    return {
      [sizeClass]: true,
      'has-error': this.isErrorVisible(inputControl)
    };
  }
}
