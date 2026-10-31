import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AuthStateService {
  private readonly TOKEN_KEY: string = 'auth_token';
  private readonly ROLE_KEY: string = 'auth_role';

  private readonly tokenState = signal<string | null>(
    sessionStorage.getItem(this.TOKEN_KEY),
  );
  private readonly roleState = signal<string | null>(
    sessionStorage.getItem(this.ROLE_KEY),
  );

  token = this.tokenState.asReadonly();
  role = this.roleState.asReadonly();

  setAuthentication(token: string, role: string): void {
    sessionStorage.setItem(this.TOKEN_KEY, token);
    sessionStorage.setItem(this.ROLE_KEY, role);

    this.tokenState.set(token);
    this.roleState.set(role);
  }

  clearAuthentication(): void {
    sessionStorage.removeItem(this.TOKEN_KEY);
    sessionStorage.removeItem(this.ROLE_KEY);

    this.tokenState.set(null);
    this.roleState.set(null);
  }
}
