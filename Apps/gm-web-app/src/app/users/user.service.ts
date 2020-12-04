import { HttpClient } from '@angular/common/http';
import { CoreEnvironment } from '@angular/compiler/src/compiler_facade_interface';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { User } from './user';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(private http: HttpClient) { }

  private url = environment.apiUrl + '/user';

  public getUsers(): Observable<Array<User>> {
    return this.http.get<Array<User>>(this.url);
  }

  public usernameInUse(username: string) {
    return this.http.get<boolean>(this.url + '/' + username + '/check');
  }

  public deleteUser(username: string) {
    return this.http.delete(this.url + '/' + username);
  }

  public addUser(user: User) {
    return this.http.post(this.url, user);
  }
}
