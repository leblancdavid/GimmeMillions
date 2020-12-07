import { Component, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { User, UserRole } from '../../user';
import { UserService } from '../../user.service';

@Component({
  selector: 'gm-new-user-dialog',
  templateUrl: './new-user-dialog.component.html',
  styleUrls: ['./new-user-dialog.component.scss']
})
export class NewUserDialogComponent implements OnInit {

  user: User;
  usernameControl = new FormControl('', [Validators.required]);
  passwordControl = new FormControl('', [Validators.required]);

  constructor(public dialogRef: MatDialogRef<NewUserDialogComponent>,
    public userService: UserService) {
    this.user = new User(0, '', '', '', '', UserRole.Default, '');
  }

  ngOnInit(): void {

  }

  onUsernameKeypress(event: Event) {
    const username = (event.target as HTMLInputElement).value;
    if (username !== '') {
      this.userService.usernameInUse(username).subscribe(x => {
        if (x) {
          this.usernameControl.setErrors({ 'inUse': true });
        } else {
          this.usernameControl.setErrors(null);
        }
      });
    }
  }

  getUsernameErrorMessage() {
    if (this.usernameControl.hasError('required')) {
      return 'You must enter a value';
    }
    return this.usernameControl.hasError('inUse') ? 'User already exists' : '';
  }
  
  getPasswordErrorMessage() {
    if (this.passwordControl.hasError('required')) {
      return 'You must enter a value';
    }
    return '';
  }

  cancel() {
    this.dialogRef.close();
  }

  save() {
    this.userService.addUser(this.user).subscribe(x => {
      this.dialogRef.close();
    },
      error => {
        console.error(error);
        this.dialogRef.close();
      });
  }
}
