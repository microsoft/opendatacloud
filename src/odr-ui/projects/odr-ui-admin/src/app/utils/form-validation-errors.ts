// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {FormGroup, ValidationErrors} from '@angular/forms';

export function getFormValidationErrors(theForm: FormGroup) {
    return {
        status: theForm.status,
        controls: Object
            .keys(theForm.controls)
            .reduce((fieldAggr, key) => {
                const controlErrors: ValidationErrors = (theForm.get(key).errors || {});
                fieldAggr[key] = Object
                    .keys(controlErrors)
                    .reduce((errAggr, keyError) => {
                        errAggr[keyError] = controlErrors[keyError];
                        return errAggr;
                    }, {});
                return fieldAggr;
            }, {})
    };
}
