import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStateService } from '../service/auth-state.service';

export const authGuard: CanActivateFn = () => {
  const authState = inject(AuthStateService);
  const router = inject(Router);

  const token = authState.token();
  const role = authState.role();

  if (token && role === 'user') {
    return true;
  }

  authState.clearAuthentication();
  return router.createUrlTree(['/login']);
};
