import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

import { ApiResponse, ApiDataResponse } from '../model/api-response';
import { UserCredential } from '../model/user-credential';
import { UserRegistration } from '../model/user-registration';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = 'https://localhost:7064/api/account';

  constructor(private http: HttpClient) { }

  login(credential: UserCredential): Observable<ApiDataResponse<string>> {
    return this.http.post<ApiDataResponse<string>>(
      `${this.apiUrl}/check`, credential
    )
  }

  register(registration: UserRegistration): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(
      `${this.apiUrl}/register`, registration
    )
  }
}
