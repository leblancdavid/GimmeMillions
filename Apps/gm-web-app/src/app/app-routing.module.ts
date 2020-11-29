import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MainComponent } from './main.component';
import { LoginComponent } from './users/login/login.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: 'first-component',
    component: MainComponent, // this is the component with the <router-outlet> in the template
    children: [
      {
        path: 'child-a', // child route path
        component: LoginComponent, // child route component that the router renders
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
