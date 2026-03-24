import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-admin-reports',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-reports.html',
  styleUrl: './admin-reports.scss'
})
export class AdminReports implements OnInit {
  private apiService = inject(ApiService);
  isExporting = false;
  isLoading = false;
  error = '';
  summary: any = null;
  filters = {
    fromDate: '',
    toDate: '',
    category: 'All Categories',
    brand: 'All Brands',
    owner: 'All Owners'
  };

  chartData = [
    { month: 'Jan', value: 45 }, { month: 'Feb', value: 60 },
    { month: 'Mar', value: 85 }, { month: 'Apr', value: 120 },
    { month: 'May', value: 95 }, { month: 'Jun', value: 140 }
  ];

  ngOnInit(): void {
    this.loadSummary();
  }

  loadSummary() {
    this.isLoading = true;
    this.error = '';
    this.apiService.getDashboardSummary().subscribe({
      next: (data: any) => {
        this.summary = {
          totalActiveProducts: data?.totalActiveProducts ?? data?.TotalActiveProducts ?? 0,
          pendingApprovals: data?.pendingApprovals ?? data?.PendingApprovals ?? 0,
          lowStockItems: data?.lowStockItems ?? data?.LowStockItems ?? 0,
          alerts: data?.alerts ?? data?.Alerts ?? []
        };
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Failed to load report summary.';
        this.summary = null;
        this.isLoading = false;
      }
    });
  }

  applyFilters() {
    this.loadSummary();
  }

  downloadReport() {
    this.isExporting = true;
    this.apiService.exportDashboardData().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'admin_report_export.csv';
        a.click();
        window.URL.revokeObjectURL(url);
        this.isExporting = false;
      },
      error: () => {
        alert('Failed to download report.');
        this.isExporting = false;
      }
    });
  }
}
