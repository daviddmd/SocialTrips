import {FormGroup} from "@angular/forms";

export function MustMatch(controlName: string, matchingControlName: string) {
  return (formGroup: FormGroup) => {
    const control = formGroup.controls[controlName];
    const matchingControl = formGroup.controls[matchingControlName];

    if (matchingControl.errors && !matchingControl.errors["mustMatch"]) {
      return;
    }

    // set error on matchingControl if validation fails
    if (control.value !== matchingControl.value) {
      matchingControl.setErrors({ mustMatch: true });
    } else {
      matchingControl.setErrors(null);
    }
  }
}

export function ValidateTripDate(beginningDate: string, endingDate: string){
  return (formGroup: FormGroup) => {
    const control = formGroup.controls[beginningDate];
    const matchingControl = formGroup.controls[endingDate];
    if (matchingControl.errors && !matchingControl.errors["validateTripDate"]) {
      return;
    }

    if (new Date(control.value) >= new Date(matchingControl.value)) {
      matchingControl.setErrors({ validateTripDate: true });
    } else {
      matchingControl.setErrors(null);
    }
  }
}
