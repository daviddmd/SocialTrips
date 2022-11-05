import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {Observable} from "rxjs";
import {environment} from "../../environments/environment";
import {ITrip} from "../shared/interfaces/entities/ITrip";

const httpOptions = {
  headers: new HttpHeaders({'Content-Type': 'application/json'})
};

@Injectable({
  providedIn: 'root'
})

export class TripService {

  constructor(private http: HttpClient) {
  }

  createTrip(form: any): Observable<any> {
    return this.http.post(environment.API_URL + "/Trips", form, httpOptions);
  }

  getAllTrips(): Observable<ITrip[]> {
    return this.http.get<ITrip[]>(environment.API_URL + "/Trips", httpOptions);
  }

  searchTrips(tripName: string, tripDescription: string, tripDestination: string): Observable<ITrip[]> {
    return this.http.post<ITrip[]>(environment.API_URL + "/Trips/Search",
      {
        tripName: tripName,
        tripDescription: tripDescription,
        tripDestination: tripDestination
      },
      httpOptions);
  }
  joinTrip(tripId: string, inviteId: string | null): Observable<ITrip> {
    return this.http.post<ITrip>(environment.API_URL + "/Trips/Join", {
      tripId: tripId,
      inviteId: inviteId
    }, httpOptions);
  }
  inviteUser(tripId: string, userId: string): Observable<any>{
    return this.http.post<ITrip>(environment.API_URL + "/Trips/Invitations", {
      tripId: tripId,
      userId: userId
    }, httpOptions);
  }
  deleteInvitation(invitationId: string): Observable<any> {
    return this.http.delete(environment.API_URL + `/Trips/Invitations/${invitationId}`, httpOptions);
  }
  getTrip(tripId: string) : Observable<ITrip>{
    return this.http.get<ITrip>(environment.API_URL + `/Trips/${tripId}`, httpOptions);
  }
  updateTripDetails(tripId: string, form : any) : Observable<any>{
    return this.http.put(environment.API_URL + `/Trips/${tripId}`, form, httpOptions);
  }
  deleteTrip(tripId: string) : Observable<any>{
    return this.http.delete(environment.API_URL + `/Trips/${tripId}`, httpOptions);
  }
  removeUserFromTrip(tripId: string, userId: string) : Observable<any>{
    return this.http.post(environment.API_URL + `/Trips/RemoveUser/`,
      {
        tripId: tripId,
        userId: userId
      },
      httpOptions);
  }
  leaveTrip(tripId: string): Observable<any>{
    return this.http.post(environment.API_URL + `/Trips/Leave/`,
      {
        tripId: tripId
      },
      httpOptions);
  }
  updateTripPicture(tripId: string, image: File): Observable<any> {
    let formData: FormData = new FormData();
    formData.append('file', image, image.name);
    return this.http.post(environment.API_URL + `/Trips/${tripId}/Picture`, formData);
  }
  deleteTripPicture(tripId: string) : Observable<any>{
    return this.http.delete(environment.API_URL + `/Trips/${tripId}/Picture`, httpOptions);
  }
}
