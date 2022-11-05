import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {environment} from "../../environments/environment";
import {Observable} from "rxjs";
import {UserRole} from "../shared/enums/UserRole";
import {IUser} from "../shared/interfaces/entities/IUser";

const httpOptions = {
  headers: new HttpHeaders({'Content-Type': 'application/json'})
};

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private http: HttpClient) {
  }

  getSelf(): Observable<IUser> {
    return this.http.get<IUser>(environment.API_URL + '/Users/Self', httpOptions);
  }

  getUser(id: string): Observable<IUser> {
    return this.http.get<IUser>(environment.API_URL + `/Users/${id}`, httpOptions);
  }

  updateSelf(form: any): Observable<any> {
    return this.http.put(environment.API_URL + "/Users/Self", form, httpOptions);
  }

  updateUser(userId: string, form: any): Observable<any> {
    return this.http.put(environment.API_URL + `/Users/${userId}`, form, httpOptions);
  }

  deleteUser(userId: string) {
    return this.http.delete(environment.API_URL + `/Users/${userId}`);
  }

  updateOwnPicture(fileToUpload: File): Observable<any> {
    let formData: FormData = new FormData();
    formData.append('file', fileToUpload, fileToUpload.name);
    return this.http.post(environment.API_URL + "/Users/Self/Picture", formData);
  }

  updateUserPicture(fileToUpload: File, userId: string): Observable<any> {
    let formData: FormData = new FormData();
    formData.append('file', fileToUpload, fileToUpload.name);
    return this.http.post(environment.API_URL + `/Users/${userId}/Picture`, formData);
  }

  getAllUsers(): Observable<IUser[]> {
    return this.http.get<IUser[]>(environment.API_URL + '/Users', httpOptions);
  }

  searchUsers(nameOrEmail: string): Observable<IUser[]> {
    return this.http.post<IUser[]>(environment.API_URL + '/Users/Search', {
        nameOrEmail
      },
      httpOptions);
  }

  updateUserRoles(userId: string, roles: UserRole[]) {
    return this.http.put(environment.API_URL + `/Users/${userId}/Roles`, {
        roles
      },
      httpOptions);
  }

  followUser(userId: string): Observable<any> {
    return this.http.put(environment.API_URL + `/Users/Following/${userId}`, httpOptions);
  }

  unfollowUser(userId: string): Observable<any> {
    return this.http.delete(environment.API_URL + `/Users/Following/${userId}`, httpOptions);
  }

  deleteOwnPicture(): Observable<any> {
    return this.http.delete(environment.API_URL + `/Users/Self/Picture`, httpOptions);
  }

  deleteUserPicture(userId: string): Observable<any> {
    return this.http.delete(environment.API_URL + `/Users/${userId}/Picture`, httpOptions);
  }
}
