import { Component, Input, OnInit } from '@angular/core';
import { NotificationsService } from '../notifications.service';

import { Notification } from '../notification';

@Component({
  selector: 'gm-notification',
  templateUrl: './notification.component.html',
  styleUrls: ['./notification.component.scss']
})
export class NotificationComponent implements OnInit {

  constructor(public notificationsService: NotificationsService) { }

  ngOnInit(): void {
  }

  dismiss(n: Notification) {
    this.notificationsService.dismiss(n);
  }
}
