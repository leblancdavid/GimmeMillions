import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MainComponent } from './main.component';
import { LoginComponent } from './users/login/login.component';
import { UserManagementComponent } from './users/user-management/user-management.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: 'main',
    component: MainComponent, // this is the component with the <router-outlet> in the template
    children: [
      {
        path: 'users',
        component: UserManagementComponent,
      },
      {
        path: 'child-b',
        component: LoginComponent, // another child route component that the router renders
      },
    ],
  },
  { path: '',   redirectTo: '/login', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
