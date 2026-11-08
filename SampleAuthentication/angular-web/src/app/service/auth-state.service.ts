import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AuthStateService {
  private readonly authenticatedState = signal(false);

  authenticated = this.authenticatedState.asReadonly();

  setAuthenticated(): void {
    this.authenticatedState.set(true);
  }

  clearAuthentication(): void {
    this.authenticatedState.set(false);
  }
}
