import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '../authentication.service';
import { User } from '../user';

@Component({
  selector: 'gm-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {

  private currentUser!: User;
  constructor(public authenticationService: AuthenticationService) {

   }

  ngOnInit(): void {
    this.currentUser = this.authenticationService.currentUserValue;
  }

}
