import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CryptoComponent } from './crypto/crypto/crypto.component';
import { MainComponent } from './main.component';
import { StocksComponent } from './stocks/stocks.component';
import { LoginComponent } from './users/login/login.component';
import { ProfileComponent } from './users/profile/profile.component';
import { UserManagementComponent } from './users/user-management/user-management.component';

const routes: Routes = [
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

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
