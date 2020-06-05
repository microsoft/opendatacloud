// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {AfterContentInit, Component, ContentChildren, EventEmitter, Input, Output, QueryList} from '@angular/core';
import {AbstractControl} from '@angular/forms';
import {ErrMsgComponent} from '../errmsg/errmsg.component';

@Component({
    selector: 'app-validated-url',
    templateUrl: './validated-url.component.html',
    styleUrls: ['./validated-url.component.scss']
})
export class ValidatedUrlComponent implements AfterContentInit {

    @ContentChildren(ErrMsgComponent, { descendants: true }) errorMessages: QueryList<ErrMsgComponent>;

    @Input() inputControl: AbstractControl;
    @Input() size: number;
    @Input() labelText: string;
    @Input() required: boolean;
    @Input() isLabelChecked: boolean;
    @Input() isLabelCheckboxVisible: boolean;
    @Output() labelChecked: EventEmitter<any> = new EventEmitter<any>();
    @Output() inputValueChanged: EventEmitter<any> = new EventEmitter<any>();

    get formGroup(): AbstractControl {
        return this.inputControl.parent;
    }

    ngAfterContentInit() {
        this.errorMessages.forEach((c, idx) => {
            c.inputControl = this.inputControl;
        });
    }

    onClickLabelCheckbox() {
        this.labelChecked.emit();
    }

    onChangeInputValue() {
        this.inputValueChanged.emit();
    }

    isErrorVisible(inputControl: AbstractControl) {
        return !inputControl.pristine && !inputControl.valid;
    }

    get sizeClass() {
        return this.size ? `col-lg-${this.size}` : 'col-lg-12';
    }

    get rootClasses() {
        const { sizeClass, inputControl } = this;
        return {
            [sizeClass]: true,
            'has-error': this.isErrorVisible(inputControl)
        };
    }
}
