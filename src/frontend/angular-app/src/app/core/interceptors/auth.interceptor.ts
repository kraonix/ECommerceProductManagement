import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../services/api.service';
import { catchError, switchMap, throwError } from 'rxjs';

let isRefreshing = false;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const apiService = inject(ApiService);
  const token = localStorage.getItem('jwt_token');

  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && token) {
        // Core validation failure, attempt background Token Refresh cycle securely.
        const refreshToken = localStorage.getItem('refresh_token');
        if (refreshToken && !isRefreshing) {
          isRefreshing = true;
          return apiService.refreshToken(token, refreshToken).pipe(
            switchMap((response: any) => {
              isRefreshing = false;
              localStorage.setItem('jwt_token', response.token);
              localStorage.setItem('refresh_token', response.refreshToken);
              
              // Retry hung request with fresh session payload
              const retriedReq = req.clone({
                setHeaders: { Authorization: `Bearer ${response.token}` }
              });
              return next(retriedReq);
            }),
            catchError((refreshErr) => {
              isRefreshing = false;
              // Cleanly drop corrupted sessions
              localStorage.clear();
              router.navigate(['/auth/login']);
              return throwError(() => refreshErr);
            })
          );
        } else {
           // Not recoverable
           localStorage.clear();
           router.navigate(['/auth/login']);
        }
      }
      return throwError(() => error);
    })
  );
};
