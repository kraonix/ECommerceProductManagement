import { Routes } from '@angular/router';
import { Layout } from './core/layout/layout';
import { Dashboard } from './features/dashboard/dashboard';
import { ProductListing } from './features/product-listing/product-listing';
import { ProductWizard } from './features/product-wizard/product-wizard';
import { StorefrontPreview } from './features/storefront-preview/storefront-preview';
import { ProductAudit } from './features/product-audit/product-audit';
import { AdminReports } from './features/admin-reports/admin-reports';
import { Login } from './features/auth/login/login';
import { CustomerProductList } from './features/customer-product-list/customer-product-list';
import { CustomerProductDetail } from './features/customer-product-detail/customer-product-detail';
import { authGuard, roleGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'auth/login', component: Login },
  {
    path: 'admin',
    component: Layout,
    canActivate: [authGuard, roleGuard],
    canActivateChild: [roleGuard],
    data: { roles: ['Admin', 'ProductManager', 'ContentExecutive'] },
    children: [
      { path: 'dashboard', component: Dashboard },
      { path: 'catalog', component: ProductListing },
      { path: 'catalog/new', component: ProductWizard },
      { path: 'preview/:id', component: StorefrontPreview },
      { path: 'audit', component: ProductAudit },
      { path: 'reports', component: AdminReports, data: { roles: ['Admin'] } },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  {
    path: 'customer',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Customer'] },
    children: [
      { path: 'products', component: CustomerProductList },
      { path: 'product/:id', component: CustomerProductDetail },
      { path: 'preview/:id', component: StorefrontPreview },
      { path: '', redirectTo: 'products', pathMatch: 'full' }
    ]
  },
  { path: '', pathMatch: 'full', redirectTo: 'auth/login' },
  { path: '**', redirectTo: 'auth/login' }
];
