import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpResponse } from '@angular/common/http';

import { ApiResponse, ApiDataResponse } from '../model/api-response';
import { UserCredential } from '../model/user-credential';
import { UserRegistration } from '../model/user-registration';
import { LogLevel } from '../utils/logger-level';
import { LoggerService } from './shared/logger.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = 'https://localhost:7064/api/account';

  constructor(
    private http: HttpClient,
    private logger: LoggerService,
  ) {}

  login(
    credential: UserCredential,
  ): Observable<HttpResponse<ApiDataResponse<string>>> {
    this.logger.logDetails(LogLevel.INFO, 'Checking user credential...');
    return this.http.post<ApiDataResponse<string>>(
      `${this.apiUrl}/check`,
      credential,
      {
        observe: 'response',
        withCredentials: true,
      },
    );
  }

  completeHandoff(
    handoff: string,
  ): Observable<HttpResponse<ApiDataResponse<string>>> {
    this.logger.logDetails(
      LogLevel.INFO,
      'Completing authentication hand-off...',
    );

    return this.http.post<ApiDataResponse<string>>(
      `${this.apiUrl}/complete-handoff`,
      { handoff },
      {
        observe: 'response',
        withCredentials: true,
      },
    );
  }

  register(
    registration: UserRegistration,
  ): Observable<HttpResponse<ApiResponse>> {
    this.logger.logDetails(LogLevel.INFO, 'Registering new user...');
    return this.http.post<ApiResponse>(
      `${this.apiUrl}/register`,
      registration,
      { observe: 'response', withCredentials: true },
    );
  }

  logout(): Observable<HttpResponse<ApiResponse>> {
    this.logger.logDetails(LogLevel.INFO, 'Logging out...');

    return this.http.post<ApiResponse>(
      `${this.apiUrl}/ng-logout`,
      {},
      {
        observe: 'response',
        withCredentials: true,
      },
    );
  }
}
