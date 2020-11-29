import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from './users/user.service';

@Component({
  selector: 'gm-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.scss']
})
export class MainComponent implements OnInit {

  constructor(public userService: UserService,
    private router: Router) { }

  ngOnInit(): void {
  }

  logout() {
    this.router.navigate(['/login']);
  }

}
