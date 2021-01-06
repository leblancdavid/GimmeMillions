import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from './users/authentication.service';
import { UserService } from './users/user.service';

@Component({
  selector: 'gm-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.scss']
})
export class MainComponent implements OnInit {

  constructor(public userService: UserService,
    public authenticationService: AuthenticationService,
    private router: Router) { }

  ngOnInit(): void {
  }

  logout() {
    this.authenticationService.logout();
    this.router.navigate(['/login']);
  }
  goToUserManagement() {
    this.router.navigate(['/main/users']);
  }
  goToProfile() {
    this.router.navigate(['/main/profile']);
  }
  goToStocks() {
    this.router.navigate(['/main/stocks']);
  }
  goToCrypto() {
    this.router.navigate(['/main/crypto']);
  }
  goToTutorials() {
    this.router.navigate(['/main/tutorials']);
  }
  goToDisclaimer() {
    this.router.navigate(['/main/disclaimer']);
  }
}
