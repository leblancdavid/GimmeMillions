import { Component, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { AuthenticationService } from '../../authentication.service';
import { User } from '../../user';

@Component({
  selector: 'gm-reset-password-dialog',
  templateUrl: './reset-password-dialog.component.html',
  styleUrls: ['./reset-password-dialog.component.scss']
})
export class ResetPasswordDialogComponent implements OnInit {
  
  oldPasswordControl = new FormControl('', [Validators.required]);
  newPasswordControl = new FormControl('', [Validators.required]);
  newPasswordConfirmControl = new FormControl('', [Validators.required]);

  constructor(public dialogRef: MatDialogRef<ResetPasswordDialogComponent>,
    public authenticationService: AuthenticationService) {

  }

  ngOnInit(): void {
  }

  cancel() {
    this.dialogRef.close();
  }

  getOldPasswordErrorMessage() {
    if (this.oldPasswordControl.hasError('required')) {
      return 'You must enter a value';
    }
    return this.oldPasswordControl.hasError('failed') ? 'Invalid password' : '';
  }

  getNewPasswordErrorMessage() {
    if (this.newPasswordControl.hasError('required')) {
      return 'You must enter a value';
    } else if(this.newPasswordControl.hasError('duplicate')) {
      return 'Password must be different';
    }
    return this.newPasswordControl.value !== this.newPasswordConfirmControl.value ? 'Passwords must match' : '';
  }

  onNewPasswordKeypress(event: Event) {
    const password = (event.target as HTMLInputElement).value;
    if (password === '') {
      this.newPasswordControl.setErrors({ 'required': true });
    } else if(password === this.oldPasswordControl.value) {
      this.newPasswordControl.setErrors({ 'duplicate': true });
    } else {
      this.newPasswordControl.setErrors(null);
    }
  }

  onConfirmPasswordKeypress(event: Event) {
    const password = (event.target as HTMLInputElement).value;
    if (password === '') {
      this.newPasswordConfirmControl.setErrors({ 'required': true });
    } else if(password !== this.newPasswordControl.value) {
      this.newPasswordConfirmControl.setErrors({ 'mismatch': true });
    } else {
      this.newPasswordConfirmControl.setErrors(null);
    }
  }

  resetPassword() {
    this.authenticationService.resetPassword(this.oldPasswordControl.value, this.newPasswordControl.value).subscribe(x => {
      this.dialogRef.close();
      this.authenticationService.logout();
    },
      error => {
       console.error(error);
       this.oldPasswordControl.setErrors({ 'failed': true });
      });
  }
}
