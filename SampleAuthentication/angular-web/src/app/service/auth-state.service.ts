import { Injectable, signal } from '@angular/core';
import { LogLevel } from '../utils/logger-level';
import { LoggerService } from './shared/logger.service';
import { SystemMessage } from 'src/app/utils/system-message';

@Injectable({
  providedIn: 'root',
})
export class AuthStateService {
  private readonly authenticatedState = signal(false);

  authenticated = this.authenticatedState.asReadonly();

  constructor(private logger: LoggerService) {}

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
  }
}
