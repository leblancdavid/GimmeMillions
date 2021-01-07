import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { NotificationComponent } from './notifications/notification/notification.component';
import { NotificationsService } from './notifications/notifications.service';
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
    public notificationsService: NotificationsService,
    private router: Router, private _snackBar: MatSnackBar) { 
    }

  ngOnInit(): void {
    this.notificationsService.notificationsChange.subscribe(() => {
      if(this.notificationsService.currentNotifications.length == 0) {
        this.closeNotifications();
      }
    })
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

  public showNotifications() {
    this._snackBar.openFromComponent(NotificationComponent, {
    });
  }

  public closeNotifications() {
    this._snackBar.dismiss();
  }
}
