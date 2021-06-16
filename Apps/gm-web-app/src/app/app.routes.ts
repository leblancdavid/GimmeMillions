import { Routes } from "@angular/router";
import { CryptoComponent } from "./crypto/crypto/crypto.component";
import { MainComponent } from "./main.component";
import { StocksComponent } from "./stocks/stocks.component";
import { DisclaimerComponent } from "./support/disclaimer/disclaimer.component";
import { TutorialsComponent } from "./support/tutorials/tutorials.component";
import { CanActivateSuperUser } from "./users/auth.guard";
import { LoginComponent } from "./users/login/login.component";
import { ProfileComponent } from "./users/profile/profile.component";
import { UserManagementComponent } from "./users/user-management/user-management.component";

export const routes: Routes = [
    { path: 'login', component: LoginComponent },
    {
      path: 'main',
      component: MainComponent,
      children: [
        {
          path: 'stocks',
          component: StocksComponent, 
        },
        {
          path: 'crypto',
          component: CryptoComponent, 
        },
        {
          path: 'users',
          component: UserManagementComponent,
          canActivate: [CanActivateSuperUser]
        },
        {
          path: 'tutorials',
          component: TutorialsComponent
        },
        {
          path: 'disclaimer',
          component: DisclaimerComponent
        },
        {
          path: 'profile',
          component: ProfileComponent,
        },
        { path: '',   redirectTo: '/main/stocks', pathMatch: 'full' }
      ],
    },
    { path: '',   redirectTo: '/login', pathMatch: 'full' }
  ];
  