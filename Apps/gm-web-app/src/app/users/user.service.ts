import { HttpClient } from '@angular/common/http';
import { CoreEnvironment } from '@angular/compiler/src/compiler_facade_interface';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(private http: HttpClient) { }

  private url = environment.apiUrl + '/user';

  public authenticate(username: string, password: string) {
    return this.http.post(this.url + '/authenticate',
      {
        username: username,
        password: password
      })
  }
}
