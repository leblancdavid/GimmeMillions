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
    }
    return this.newPasswordControl.value !== this.newPasswordConfirmControl.value ? 'Passwords must match' : '';
  }

  resetPassword() {
    //this.authenticationService.addUser(this.user).subscribe(x => {
    //  this.dialogRef.close();
    //},
    //  error => {
    //    console.error(error);
    //    this.dialogRef.close();
    //  });
  }
}
