import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem('jwt_token');
  
  if (token) {
    return true;
  }
  
  router.navigate(['/auth/login']);
  return false;
};

export const roleGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem('jwt_token');
  const userRole = (localStorage.getItem('user_role') || '').trim();
  const allowedRoles = (route.data?.['roles'] as string[] | undefined) ?? [];

  if (!token) {
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
