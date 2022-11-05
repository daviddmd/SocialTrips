import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {Observable} from "rxjs";
import {ITrip} from "../shared/interfaces/entities/ITrip";
import {IUser} from "../shared/interfaces/entities/IUser";
import {environment} from "../../environments/environment";
import {IActivity} from "../shared/interfaces/entities/IActivity";
import {IActivityCreate} from "../shared/interfaces/IActivityCreate";
import {IActivityUpdate} from "../shared/interfaces/IActivityUpdate";
import {IActivityTransportSearch} from "../shared/interfaces/IActivityTransportSearch";
import {IActivityTransport} from "../shared/interfaces/entities/IActivityTransport";
const httpOptions = {
  headers: new HttpHeaders({'Content-Type': 'application/json'})
};
@Injectable({
  providedIn: 'root'
})
export class ActivityService {

  constructor(private http: HttpClient) { }
  createActivity(activity: IActivityCreate) :Observable<ITrip>{
    return this.http.post<ITrip>(environment.API_URL + '/Activity',activity, httpOptions);
  }
  updateActivity(activityId : string, activity: IActivityUpdate) : Observable<ITrip>{
    return this.http.put<ITrip>(environment.API_URL + `/Activity/${activityId}`,activity, httpOptions);
  }
  deleteActivity(activityId: string) : Observable<ITrip>{
    return this.http.delete<ITrip>(environment.API_URL + `/Activity/${activityId}`, httpOptions);
  }
  searchTransport(transportSearch: IActivityTransportSearch) : Observable<IActivityTransport[]>{
    return this.http.post<IActivityTransport[]>(environment.API_URL + '/Activity/SearchTransport',transportSearch, httpOptions);

  }
}
