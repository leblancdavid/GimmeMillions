import { Injectable } from '@angular/core';
import { Notification } from './notification';

@Injectable({
  providedIn: 'root'
})
export class NotificationsService {

  constructor() { 
    this.currentNotifications = new Array<Notification>();
    this.currentNotifications.push(new Notification(`Welcome to the <b>Gimmillions</b> app!`));
  }

  public currentNotifications: Array<Notification>;
}
