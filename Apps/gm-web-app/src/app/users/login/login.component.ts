import { Component, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'gm-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  usernameControl = new FormControl('', [Validators.required]);
  passwordControl = new FormControl('', [Validators.required]);

  constructor() { }
  ngOnInit(): void {
  }

  getUsernameErrorMessage() {
    if (this.usernameControl.hasError('required')) {
      return 'You must enter a value';
    }
    return this.usernameControl.hasError('failed') ? 'Invalid username or password' : '';
  }
  getPasswordErrorMessage() {
    if (this.passwordControl.hasError('required')) {
      return 'You must enter a value';
    }
    return '';
  }

}
