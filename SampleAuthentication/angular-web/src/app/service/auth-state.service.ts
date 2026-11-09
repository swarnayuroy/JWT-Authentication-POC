import { Injectable, signal } from '@angular/core';
import { LogLevel } from '../utils/logger-level';
import { LoggerService } from './shared/logger.service';
import { SystemMessage } from 'src/app/utils/system-message';

@Injectable({
  providedIn: 'root',
})
export class AuthStateService {
  private readonly authenticatedState = signal(false);
  private readonly userIdState = signal<string | null>(null);

  authenticated = this.authenticatedState.asReadonly();
  userId = this.userIdState.asReadonly();

  constructor(private logger: LoggerService) {}

  setUserId(userId: string): void {
    this.userIdState.set(userId);
    sessionStorage.setItem('userId', userId);
  }

  restoreUserId(): void {
    const userId = sessionStorage.getItem('userId');

    if (userId) {
      this.userIdState.set(userId);
      this.authenticatedState.set(true);
    }
  }

  setAuthenticated(message?: string): void {
    this.logger.logDetails(
      LogLevel.INFO,
      message || SystemMessage.LOGIN_SUCCESS,
    );
    this.authenticatedState.set(true);
  }

  clearAuthentication(message?: string): void {
    this.logger.logDetails(
      LogLevel.INFO,
      message || SystemMessage.LOGOUT_SUCCESS,
    );
    this.authenticatedState.set(false);
    this.userIdState.set(null);
    sessionStorage.removeItem('userId');
  }
}
