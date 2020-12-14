import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatNativeDateModule } from '@angular/material/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from '../material/material.module';
import { LoginComponent } from './login/login.component';
import { UserService } from './user.service';
import { UserManagementComponent } from './user-management/user-management.component';
import { AuthenticationService } from './authentication.service';
import { ProfileComponent } from './profile/profile.component';
import { ResetPasswordDialogComponent } from './profile/reset-password-dialog/reset-password-dialog.component';
import { NewUserDialogComponent } from './user-management/new-user-dialog/new-user-dialog.component';
import { CanActivateSuperUser } from './auth.guard';

@NgModule({
  declarations: [LoginComponent, UserManagementComponent, NewUserDialogComponent, ProfileComponent, ResetPasswordDialogComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    HttpClientModule,
    MatNativeDateModule,
    ReactiveFormsModule,
    MaterialModule
  ],
  providers: [ UserService, AuthenticationService, CanActivateSuperUser ]
})
export class UsersModule { }
