import { EventEmitter, Injectable } from '@angular/core';
import { Notification } from './notification';

@Injectable({
  providedIn: 'root'
})
export class NotificationsService {

  constructor() {
    this.currentNotifications = new Array<Notification>();
    this.currentNotifications.push(new Notification(`Welcome to the <b>Gimmillions</b> app! We are still very
    early in development but we appreciate your support and time. All feedback is welcomed and is very valuable to us.
    If you have ideas for improvement or find bugs, please sign up and submit feedback/enhancement ideas/bugs
    <a href="https://github.com/leblancdavid/GimmeMillions/issues" class="light-accent-text"
            target="_blank">here</a>`));
  }

  public currentNotifications: Array<Notification>;
  public notificationsChange = new EventEmitter<string>();
  

  public dismiss(n: Notification) {
    const index = this.currentNotifications.indexOf(n);
    if(index > -1) {
      this.currentNotifications.splice(index, 1);
      this.notificationsChange.emit('dismiss');
    }
    //if(this.currentNotifications.length == 0) {
    //  this.close();
    //}
  }

  
}
