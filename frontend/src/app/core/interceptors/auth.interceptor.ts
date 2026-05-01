import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const excludedUrls = ['/auth/login', '/auth/register', '/auth/refresh-token'];
  const isExcluded = excludedUrls.some(url => req.url.includes(url));

  if (isExcluded) {
    return next(req);
  }

  const authService = inject(AuthService);
  const token = authService.getStoredToken();

  if (!token) {
    return next(req).pipe(
      catchError((error: HttpErrorResponse) => handleHttpError(error))
    );
  }

  const authReq = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => handleHttpError(error))
  );
};

function handleHttpError(error: HttpErrorResponse) {
  const router = inject(Router);
  const authService = inject(AuthService);

  if (error.status === 401) {
    authService.logout();
    const currentUrl = router.url;
    if (currentUrl && !currentUrl.startsWith('/login')) {
      router.navigate(['/login'], { queryParams: { returnUrl: currentUrl } });
    }
  }

  return throwError(() => error);
}
