import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Notification } from './notification';
import { NotificationComponent } from './notification/notification.component';

@Injectable({
  providedIn: 'root'
})
export class NotificationsService {

  constructor(private _snackBar: MatSnackBar) { 
    this.currentNotifications = new Array<Notification>();
    this.currentNotifications.push(new Notification(`Welcome to the <b>Gimmillions</b> app! We are still very
    early in development but we appreciate your support and time. All feedback is welcomed and is very valuable to us.
    If you have ideas for improvement or find bugs, please sign up and submit feedback/enhancement ideas/bugs
    <a href="https://github.com/leblancdavid/GimmeMillions/issues" class="light-accent-text"
            target="_blank">here</a>`));
  }

  public currentNotifications: Array<Notification>;

  public show() {
    this._snackBar.openFromComponent(NotificationComponent, {
    });
  }
}
