import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User, UserRole } from './user';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {

  private currentUserSubject: BehaviorSubject<User>;
  public currentUser: Observable<User>;

  constructor(private http: HttpClient,
    private router: Router) {
    const currentUserStr = localStorage.getItem('currentUser');
    if(currentUserStr) {
      const userJson = JSON.parse(currentUserStr);
      const user = new User(userJson.id, userJson.firstName, userJson.lastName, 
        userJson.username, userJson.password, userJson.role,
        userJson.stocksWatchlistString);
      user.authdata = userJson.authdata;
      this.currentUserSubject = new BehaviorSubject<User>(user);
    } else {
      this.currentUserSubject = new BehaviorSubject<User>(new User(-1, '','','','',UserRole.Default,''));
    }
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User {
    return this.currentUserSubject.value;
  }

  login(username: string, password: string) {
    return this.http.post<User>(environment.apiUrl + '/user/authenticate', { username: username, password: password })
      .pipe(map(user => {
        // store user details and basic auth credentials in local storage to keep user logged in between page refreshes
        user.authdata = window.btoa(username + ':' + password);
        localStorage.setItem('currentUser', JSON.stringify(user));
        this.currentUserSubject.next(user);
        return user;
      }));
  }

  logout() {
    // remove user from local storage to log user out
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(new User(-1, '','','','',UserRole.Default,''));
    this.router.navigate(['/login']);
  }

  resetPassword(oldPassword: string, newPassword: string) {
    return this.http.put(environment.apiUrl + '/user/reset', 
    {
      username: this.currentUserSubject.value.username,
      oldPassword: oldPassword,
      newPassword: newPassword
    });
  }

  addToWatchlist(symbol: string) {
    const user = this.currentUserValue;
    return this.http.put(environment.apiUrl + '/user/watchlist/add',
    {
      username: user.username,
      symbols: [ symbol ]
    }).pipe(map(x => {
      user.addToWatchlist(symbol);
      return user;
    }));
  }

  removeFromWatchlist(symbol: string) {
    const user = this.currentUserValue;
    return this.http.put(environment.apiUrl + '/user/watchlist/remove',
    {
      username: user.username,
      symbols: [ symbol ]
    }).pipe(map(x => {
      user.removeFromWatchlist(symbol);
      return user;
    }));
  }



}
