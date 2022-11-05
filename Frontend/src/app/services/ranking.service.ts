import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import {IRanking} from "../shared/interfaces/entities/IRanking";

const httpOptions = {
  headers: new HttpHeaders({'Content-Type': 'application/json'})
};

@Injectable({
  providedIn: 'root'
})
export class RankingService {

  constructor(private http: HttpClient) { }

  getAllRankings(): Observable<IRanking[]> {
    return this.http.get<IRanking[]>(environment.API_URL + `/Rankings`, httpOptions);
  }

  createRanking(form: any): Observable<any> {
    return this.http.post(environment.API_URL + "/Rankings", form, httpOptions);
  }

  getRanking(id: string): Observable<IRanking> {
    return this.http.get<IRanking>(environment.API_URL + `/Rankings/${id}`, httpOptions);
  }

  removeRanking(id: string): Observable<any> {
    return this.http.delete(environment.API_URL + `/Rankings/${id}`, httpOptions);
  }

  updateRanking(id: string, ranking: IRanking): Observable<any> {
    return this.http.put(environment.API_URL + `/Rankings/${id}`, ranking, httpOptions);
  }
}
