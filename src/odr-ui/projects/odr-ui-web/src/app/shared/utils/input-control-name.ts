import { memoize } from 'lodash';
import { AbstractControl } from '@angular/forms';

export const getInputControlName = memoize((control: AbstractControl) => {
    const entry = Object
        .entries(control.parent.controls)
        .find(([key, val]) => val === control);
    if (!entry) {
        throw new Error('Name not found for input control.');
    }
    return entry[0];
});

(<any>getInputControlName).cache = new WeakMap();
