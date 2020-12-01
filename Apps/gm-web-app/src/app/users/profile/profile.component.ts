import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AuthenticationService } from '../authentication.service';
import { User } from '../user';

@Component({
  selector: 'gm-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {

  private currentUser!: User;
  constructor(public authenticationService: AuthenticationService, public dialog: MatDialog) {

   }

  ngOnInit(): void {
    this.currentUser = this.authenticationService.currentUserValue;
  }

  resetPassword() {

  }
}
