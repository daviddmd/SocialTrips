import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {IUser} from "../shared/interfaces/entities/IUser";
import {environment} from "../../environments/environment";
import {IGroup} from "../shared/interfaces/entities/IGroup";
import {Observable} from "rxjs";
import {UserGroupRole} from "../shared/enums/UserGroupRole";

const httpOptions = {
  headers: new HttpHeaders({'Content-Type': 'application/json'})
};

@Injectable({
  providedIn: 'root'
})
export class GroupService {

  constructor(private http: HttpClient) {
  }

  getGroup(groupId: string): Observable<IGroup> {
    return this.http.get<IGroup>(environment.API_URL + `/Groups/${groupId}`, httpOptions);
  }

  getAllGroups(): Observable<IGroup[]> {
    return this.http.get<IGroup[]>(environment.API_URL + `/Groups`, httpOptions);
  }

  createGroup(form: any): Observable<any> {
    return this.http.post(environment.API_URL + "/Groups", form, httpOptions);
  }

  updateGroup(groupId: string, form: any): Observable<any> {
    return this.http.put(environment.API_URL + `/Groups/${groupId}`, form, httpOptions);
  }

  deleteGroup(groupId: string): Observable<any> {
    return this.http.delete(environment.API_URL + `/Groups/${groupId}`, httpOptions);
  }

  joinGroup(groupId: string, inviteId: string | null): Observable<any> {
    return this.http.post(environment.API_URL + "/Groups/Join", {
      groupId: groupId,
      inviteId: inviteId
    }, httpOptions);
  }

  leaveGroup(groupId: string): Observable<any> {
    return this.http.post(environment.API_URL + "/Groups/Leave", {
      groupId: groupId,
    }, httpOptions);
  }

  inviteUser(groupId: string, userId: string): Observable<any> {
    return this.http.post(environment.API_URL + "/Groups/Invitations", {
      groupId: groupId,
      userId: userId
    }, httpOptions);
  }

  deleteInvitation(invitationId: string): Observable<any> {
    return this.http.delete(environment.API_URL + `/Groups/Invitations/${invitationId}`, httpOptions);
  }

  updateUserRole(groupId: string, userId: string, userGroupRole: UserGroupRole): Observable<any> {
    return this.http.post(environment.API_URL + "/Groups/UpdateUserRole",
      {groupId: groupId, userId: userId, userGroupRole: userGroupRole},
      httpOptions);
  }

  removeUser(groupId: string, userId: string): Observable<any> {
    return this.http.post(environment.API_URL + "/Groups/RemoveUser",
      {
        groupId: groupId,
        userId: userId
      },
      httpOptions);
  }

  updateGroupPicture(groupId: string, image: File): Observable<any> {
    let formData: FormData = new FormData();
    formData.append('file', image, image.name);
    return this.http.post(environment.API_URL + `/Groups/${groupId}/Picture`, formData);
  }
  deleteGroupPicture(groupId: string) : Observable<any>{
    return this.http.delete(environment.API_URL + `/Groups/${groupId}/Picture`, httpOptions);
  }

  banUser(groupId: string, userId: string, banUntil: Date | null, banReason: string, hidePosts: boolean): Observable<any> {
    return this.http.post(environment.API_URL + "/Groups/Ban", {
        groupId: groupId,
        userId: userId,
        banUntil: banUntil,
        banReason: banReason,
        hidePosts: hidePosts
      },
      httpOptions);
  }

  unbanUser(banId: string) {
    return this.http.delete(environment.API_URL + `/Groups/Ban/${banId}`, httpOptions);
  }
}
