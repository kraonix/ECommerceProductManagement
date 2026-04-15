import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

/** Returns true if the JWT token exists and has not expired. */
function isTokenValid(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    // exp is in seconds; Date.now() is in milliseconds
    return payload.exp * 1000 > Date.now();
  } catch {
    return false;
  }
}

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem('jwt_token');

  if (token && isTokenValid(token)) {
    return true;
  }

  // Token missing or expired — clear stale session and redirect
  localStorage.clear();
  router.navigate(['/auth/login']);
  return false;
};

export const roleGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem('jwt_token');
  const userRole = (localStorage.getItem('user_role') || '').trim();
  const allowedRoles = (route.data?.['roles'] as string[] | undefined) ?? [];

  if (!token || !isTokenValid(token)) {
    localStorage.clear();
    router.navigate(['/auth/login']);
    return false;
  }

  if (allowedRoles.length === 0) {
    return true;
  }

  if (allowedRoles.includes(userRole)) {
    return true;
  }

  const fallback = userRole === 'Customer' ? '/customer/products' : '/admin/dashboard';
  router.navigate([fallback]);
  return false;
};
