import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';

import { ApiDataResponse } from '../model/api-response';
import { UserDetail } from '../model/user-detail.model';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = 'https://localhost:7064/api/user';

  constructor(private http: HttpClient) {}

  getById(
    userId: string,
  ): Observable<HttpResponse<ApiDataResponse<UserDetail>>> {
    return this.http.get<ApiDataResponse<UserDetail>>(
      `${this.apiUrl}/get/${userId}`,
      {
        observe: 'response',
        withCredentials: true,
      },
    );
  }
}
