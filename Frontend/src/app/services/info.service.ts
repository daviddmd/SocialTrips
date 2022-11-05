import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { IRecommendations } from '../shared/interfaces/IRecommendations';
import { IStatistics } from '../shared/interfaces/IStatistics';

const httpOptions = {
  headers: new HttpHeaders({'Content-Type': 'application/json'})
};

@Injectable({
  providedIn: 'root'
})
export class InfoService {

  constructor(private http: HttpClient) { }

  getAllStatistics(): Observable<IStatistics> {
    return this.http.get<IStatistics>(environment.API_URL + `/info/Statistics`, httpOptions);
  }

  getRecommendations(): Observable<IRecommendations> {
    return this.http.get<IRecommendations>(environment.API_URL + `/info/Recommendations`, httpOptions);
  }
}
