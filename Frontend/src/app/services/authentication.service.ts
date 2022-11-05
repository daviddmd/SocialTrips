import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {Observable, share} from 'rxjs';
import {JwtHelperService} from '@auth0/angular-jwt';
//MUDAR PARA O ENV REAL QUANDO ENTRAR EM PROD
import {environment} from "../../environments/environment";

const httpOptions = {
  headers: new HttpHeaders({'Content-Type': 'application/json'})
};
const jwtHelperService = new JwtHelperService();

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {
  constructor(private http: HttpClient) {
  }

  login(emailOrUsername: string, password: string, rememberMe: boolean): Observable<any> {
    return this.http.post(environment.API_URL + '/Authentication/Login', {
      emailOrUsername,
      password,
      rememberMe
    }, httpOptions);
  }
  register(params: any): Observable<any>{
    return this.http.post(environment.API_URL + '/Authentication/Register', params, httpOptions);
  }
  forgotPassword(emailOrUsername: string): Observable<any>{
    return this.http.post(environment.API_URL + '/Authentication/ForgotPassword', {
      emailOrUsername
    }, httpOptions);
  }
  resendConfirmationEmail(emailOrUsername: string): Observable<any>{
    return this.http.post(environment.API_URL + '/Authentication/ResendEmailConfirmation', {
      emailOrUsername
    }, httpOptions);
  }
  confirmEmail(userId: string, emailConfirmationToken: string) : Observable<any>{
    return this.http.post(environment.API_URL + '/Authentication/ConfirmEmail', {
      userId,
      emailConfirmationToken
    }, httpOptions);
  }
  resetPassword(userId: string, passwordResetToken: string, newPassword: string) : Observable<any>{
    return this.http.post(environment.API_URL + '/Authentication/ResetPassword', {
      userId,
      passwordResetToken,
      newPassword
    }, httpOptions);
  }
  storeToken(token: string){
    localStorage.clear();
    localStorage.setItem("token", token);
  }
  logout() {
    localStorage.clear();
  }

  getToken(): string {
    return localStorage.getItem("token")!;
  }

  isLoggedIn(): boolean {
    if (localStorage.getItem("token") === null) {
      this.logout();
      return false;
    }
    let isExpired = jwtHelperService.isTokenExpired(localStorage.getItem("token")!);
    if (isExpired) {
      this.logout();
      return false;
    }
    return true;
  }

  isAdmin(): boolean {
    if (localStorage.getItem("token") === null) {
      return false;
    }
    let decodedToken = jwtHelperService.decodeToken(localStorage.getItem("token")!);
    let roles: string[] = decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
    return roles.includes("ADMIN");
  }

  getUserName(): string {
    if (localStorage.getItem("token") === null) {
      return "";
    }
    let decodedToken = jwtHelperService.decodeToken(localStorage.getItem("token")!);
    return decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];
  }

  getUserId(): string {
    if (localStorage.getItem("token") === null) {
      return "";
    }
    let decodedToken = jwtHelperService.decodeToken(localStorage.getItem("token")!);
    return decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/serialnumber"];
  }

  getRoles(): string[] {
    if (localStorage.getItem("token") === null) {
      return [];
    }
    let decodedToken = jwtHelperService.decodeToken(localStorage.getItem("token")!);
    return decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
  }
}
