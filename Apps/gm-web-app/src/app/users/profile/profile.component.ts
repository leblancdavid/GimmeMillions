import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AuthenticationService } from '../authentication.service';
import { User } from '../user';
import { ResetPasswordDialogComponent } from './reset-password-dialog/reset-password-dialog.component';

@Component({
  selector: 'gm-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {

  public currentUser!: User;
  constructor(public authenticationService: AuthenticationService, public dialog: MatDialog) {

   }

  ngOnInit(): void {
    this.currentUser = this.authenticationService.currentUserValue;
  }

  resetPassword() {
    const dialogRef = this.dialog.open(ResetPasswordDialogComponent, {
      disableClose: true
    });
    dialogRef.afterClosed().subscribe(x => {
    });
  }
}
