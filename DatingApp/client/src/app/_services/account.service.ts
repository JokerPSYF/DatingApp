import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = 'https://localhost:7244/api/';
  private currentUserSource = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) { }


  isLogged(){
    return localStorage.length > 0 ? true : false;
  }


  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map((response: User) => {

        const user = response;
        if(user){
          console.log('user true');
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);
        }
        
      })
    );
  }

  register(model: any){
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map(user => {
        if (user){ 
        localStorage.setItem('user', JSON.stringify(user));
        this.currentUserSource.next(user);
        }
        return user;
      }
    )
    )
  }

  setCurrentUser(user: User) {
    console.log('account set current user');
    this.currentUserSource.next(user);
  }

  logout() {
    console.log('account logout');

    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }
}
