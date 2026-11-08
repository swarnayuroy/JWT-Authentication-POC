import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url.startsWith('https://localhost:7064/api/')) {
    const authReq = req.clone({ withCredentials: true });

    return next(authReq);
  }
  return next(req);
};
