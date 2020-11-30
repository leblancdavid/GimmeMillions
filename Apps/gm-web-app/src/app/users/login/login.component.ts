import { Component, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthenticationService } from '../authentication.service';
@Component({
  selector: 'gm-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  usernameControl = new FormControl('', [Validators.required]);
  passwordControl = new FormControl('', [Validators.required]);

  constructor(private userService: AuthenticationService,
    private router: Router) { }
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

  authenticate() {
    this.userService.login(this.usernameControl.value, this.passwordControl.value).subscribe(x => {
      //login success
      this.router.navigate(['/main']);
    },
    error => {
      this.passwordControl.setValue('');
      this.usernameControl.setErrors({ 'failed' : true });
    })
  }

}
