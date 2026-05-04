import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../services/api.service';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError, timeout } from 'rxjs';

// Shared refresh state — guards against concurrent 401s firing multiple refresh calls
let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

/** Auth endpoints that should never trigger a token refresh on 401. */
const AUTH_PASSTHROUGH_PATHS = [
  '/auth/login',
  '/auth/signup',
  '/auth/refresh-token',
  '/auth/forgot-password',
  '/auth/reset-password',
];

function isAuthEndpoint(url: string): boolean {
  return AUTH_PASSTHROUGH_PATHS.some(path => url.includes(path));
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const apiService = inject(ApiService);
  const token = localStorage.getItem('jwt_token');

  // Never attach a Bearer token to auth endpoints — they don't need it
  // and attaching one causes confusing 401 replays on wrong-password errors.
  const isAuthCall = isAuthEndpoint(req.url);

  let authReq = req;
  if (token && !isAuthCall) {
    authReq = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Only attempt token refresh for 401s on protected (non-auth) endpoints
      // where we actually have a token that may have expired.
      if (error.status === 401 && token && !isAuthCall) {
        const refreshToken = localStorage.getItem('refresh_token');

        if (!refreshToken) {
          localStorage.clear();
          router.navigate(['/auth/login']);
          return throwError(() => error);
        }

        if (!isRefreshing) {
          isRefreshing = true;
          refreshTokenSubject.next(null);

          return apiService.refreshToken(token, refreshToken).pipe(
            switchMap((response: any) => {
              isRefreshing = false;
              localStorage.setItem('jwt_token', response.token);
              localStorage.setItem('refresh_token', response.refreshToken);
              refreshTokenSubject.next(response.token);

              return next(req.clone({
                setHeaders: { Authorization: `Bearer ${response.token}` }
              }));
            }),
            catchError((refreshErr) => {
              isRefreshing = false;
              refreshTokenSubject.next('__failed__'); // unblock queued requests so they can fail cleanly
              localStorage.clear();
              router.navigate(['/auth/login']);
              return throwError(() => refreshErr);
            })
          );
        } else {
          // Queue behind the in-flight refresh — wait up to 10s then bail
          return refreshTokenSubject.pipe(
            filter(t => t !== null),
            take(1),
            timeout(10000),
            switchMap(newToken => {
              if (newToken === '__failed__') {
                return throwError(() => error);
              }
              return next(req.clone({
                setHeaders: { Authorization: `Bearer ${newToken}` }
              }));
            }),
            catchError(() => {
              localStorage.clear();
              router.navigate(['/auth/login']);
              return throwError(() => error);
            })
          );
        }
      }

      return throwError(() => error);
    })
  );
};
