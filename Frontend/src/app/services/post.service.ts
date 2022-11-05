import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {Observable} from "rxjs";
import {IPost} from "../shared/interfaces/entities/IPost";
import {environment} from "../../environments/environment";

const httpOptions = {
  headers: new HttpHeaders({'Content-Type': 'application/json'})
};

@Injectable({
  providedIn: 'root'
})
export class PostService {

  constructor(private http: HttpClient) {
  }

  createPost(form: any): Observable<IPost> {
    return this.http.post<IPost>(environment.API_URL + '/Posts', form, httpOptions);
  }

  updatePost(postId: string, form: any): Observable<IPost> {
    return this.http.put<IPost>(environment.API_URL + `/Posts/${postId}`, form, httpOptions);
  }

  deletePost(postId: string): Observable<any> {
    return this.http.delete(environment.API_URL + `/Posts/${postId}`, httpOptions);
  }

  addAttachmentPost(postId: string, attachment: File): Observable<IPost> {
    let formData: FormData = new FormData();
    formData.append('postId', postId);
    formData.append('file', attachment, attachment.name);
    return this.http.post<IPost>(environment.API_URL + '/Posts/Attachment', formData);
  }

  remoteAttachmentPost(attachmentId: string): Observable<any> {
    return this.http.delete(environment.API_URL + `/Posts/Attachment/${attachmentId}`, httpOptions);
  }

  getAllPosts(): Observable<IPost[]> {
    return this.http.get<IPost[]>(environment.API_URL + '/Posts', httpOptions);
  }

  getPost(postId: string): Observable<IPost> {
    return this.http.get<IPost>(environment.API_URL + `/Posts/${postId}`, httpOptions);
  }
}
