// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ValidatorFn, FormGroup } from "@angular/forms";
import { from } from "rxjs";
import { tap, mergeMap } from 'rxjs/operators';

// From https://medium.com/ngx/3-ways-to-implement-conditional-validation-of-reactive-forms-c59ed6fc3325
//
// Example:
//
// conditionalValidator(
//   () => this.myForm.get("myTextField").value.includes("Illuminati"),
//   Validators.compose([Validators.required, Validators.pattern(/.*mason.*/)])
// );

export function conditionalValidator(
  predicate: () => boolean,
  validator: ValidatorFn,
  errorNamespace?: string
): ValidatorFn {
  return formControl => {
    if (!formControl.parent) {
      return null;
    }
    let error = null;
    if (predicate()) {
      error = validator(formControl);
    }
    if (errorNamespace && error) {
      const customError = {};
      customError[errorNamespace] = error;
      error = customError;
    }
    return error;
  };
}

export function updateValidations(
  form: FormGroup,
  onValueChangeFor: string | string[],
  updateValidationsFor: string | string[]
) {
  const valueChangeNames: string[] =
    typeof onValueChangeFor === "string"
      ? [onValueChangeFor]
      : onValueChangeFor;
  const validationNames: string[] =
    typeof updateValidationsFor === "string"
      ? [updateValidationsFor]
      : updateValidationsFor;

  return from(valueChangeNames).pipe(
    mergeMap(name => form.get(name).valueChanges),
    tap(() => {
      for(const name of validationNames) {
        form.get(name).updateValueAndValidity();
      }
    })
  ).subscribe();
}
