import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-product-audit',
  imports: [CommonModule, FormsModule],
  templateUrl: './product-audit.html',
  styleUrl: './product-audit.scss',
})
export class ProductAudit implements OnInit {
  private apiService = inject(ApiService);

  productId = 1;
  searchTerm = '';
  isLoading = false;
  error = '';
  entries: any[] = [];
  selectedEntry: any = null;

  ngOnInit(): void {
    this.loadAudit();
  }

  loadAudit(): void {
    this.error = '';
    this.isLoading = true;
    this.apiService.getProductAuditHistory(this.productId).subscribe({
      next: (data: any) => {
        const items = Array.isArray(data) ? data : data?.items || data?.data || [];
        this.entries = items.map((entry: any) => ({
          productId: entry.productId ?? entry.ProductId ?? this.productId,
          action: entry.action ?? entry.Action ?? 'Unknown',
          details: entry.details ?? entry.Details ?? '',
          actionBy: entry.actionBy ?? entry.ActionBy ?? 'System',
          timestamp: entry.timestamp ?? entry.Timestamp ?? new Date().toISOString()
        }));
        this.selectedEntry = this.entries[0] || null;
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Failed to load product audit history.';
        this.entries = [];
        this.selectedEntry = null;
        this.isLoading = false;
      }
    });
  }

  selectEntry(entry: any): void {
    this.selectedEntry = entry;
  }

  get filteredEntries(): any[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.entries;

    return this.entries.filter((entry) =>
      `${entry.action} ${entry.actionBy} ${entry.details}`.toLowerCase().includes(term)
    );
  }
}
