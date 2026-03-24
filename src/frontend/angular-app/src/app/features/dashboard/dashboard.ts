import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { catchError, finalize, of } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  private apiService = inject(ApiService);

  summary: any = null;
  recentProducts: any[] = [];
  userRole = (localStorage.getItem('user_role') || '').trim();
  isAdmin = false;
  loading = true;
  error = '';

  ngOnInit() {
    const token = localStorage.getItem('jwt_token');
    this.isAdmin = this.userRole.toLowerCase() === 'admin';

    if (!token) {
      this.error = 'User not authenticated. Please login again.';
      this.loading = false;
      return;
    }

    this.loadDashboard();
  }


  loadDashboard() {
    this.loading = true;
    this.error = '';
    let productsFailed = false;
    let summaryFailed = false;

    this.apiService.getProducts().pipe(
      catchError((err) => {
        console.error('Products error:', err);
        productsFailed = true;
        return of([]);
      }),
      finalize(() => {
        this.loading = false;
        if (productsFailed && (!this.isAdmin || summaryFailed)) {
          this.error = 'Unable to load dashboard data. Verify gateway and services are running.';
        }
      })
    ).subscribe({
      next: (data: any) => {
        const products = this.normalizeProducts(data);
        this.recentProducts = products.slice(0, 5);
      }
    });

    if (this.isAdmin) {
      this.apiService.getDashboardSummary().pipe(
        catchError((err) => {
          console.error('Dashboard summary error:', err);
          summaryFailed = true;
          return of(null);
        })
      ).subscribe({
        next: (data: any) => {
          this.summary = this.normalizeSummary(data);
        }
      });
    }
  }

  private normalizeSummary(data: any) {
    if (!data) {
      return {
        totalActiveProducts: 0,
        pendingApprovals: 0,
        lowStockItems: 0,
        alerts: []
      };
    }

    return {
      totalActiveProducts: data.totalActiveProducts ?? data.TotalActiveProducts ?? 0,
      pendingApprovals: data.pendingApprovals ?? data.PendingApprovals ?? 0,
      lowStockItems: data.lowStockItems ?? data.LowStockItems ?? 0,
      alerts: data.alerts ?? data.Alerts ?? []
    };
  }

  private normalizeProducts(data: any) {
    const products = Array.isArray(data) ? data : data?.items || data?.data || [];
    return products.map((p: any) => ({
      id: p.id ?? p.Id ?? 0,
      name: p.name ?? p.Name ?? 'Untitled Product',
      sku: p.sku ?? p.Sku ?? 'N/A',
      publishStatus: p.publishStatus ?? p.PublishStatus ?? 'Draft'
    }));
  }

  getStatusClass(status: string | null | undefined): string {
    return (status || 'draft').toLowerCase();
  }

  exportData() {
    this.apiService.exportDashboardData().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');

        a.href = url;
        a.download = 'dashboard_export.csv';

        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);

        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Export error:', err);
        alert('Export failed. Please try again.');
      }
    });
  }
}