import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-product-listing',
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './product-listing.html',
  styleUrl: './product-listing.scss'
})
export class ProductListing implements OnInit {
  products: any[] = [];
  filteredProducts: any[] = [];
  searchTerm = '';
  isLoading = true;
  isUpdatingStatus = false;
  actionMessage = '';
  actionError = '';
  loadError = '';
  userRole = (localStorage.getItem('user_role') || '').trim();
  isAdmin = false;
  isProductManager = false;
  isContentExecutive = false;
  selectedStatusByProduct: Record<number, string> = {};
  statusRemarkByProduct: Record<number, string> = {};

  private apiService = inject(ApiService);

  ngOnInit() {
    this.isAdmin = this.userRole === 'Admin';
    this.isProductManager = this.userRole === 'ProductManager';
    this.isContentExecutive = this.userRole === 'ContentExecutive';

    this.apiService.getProducts().subscribe({
      next: (data: any) => {
        const products = Array.isArray(data) ? data : data?.items || data?.data || [];
        this.products = products.map((p: any) => this.normalizeProduct(p));
        this.filteredProducts = [...this.products];
        this.isLoading = false;
      },
      error: (err) => {
        this.loadError = err?.status === 401
          ? 'Unauthorized request. Please login again.'
          : 'Unable to load catalog data. Verify gateway and catalog service are running.';
        this.isLoading = false;
      }
    });
  }

  filterProducts() {
    const term = this.searchTerm.toLowerCase().trim();
    this.filteredProducts = this.products.filter(p => 
      p.name.toLowerCase().includes(term) || 
      p.sku.toLowerCase().includes(term)
    );
  }

  updateStatus(product: any) {
    if (!this.isAdmin) {
      this.actionError = 'Only Admin can change product status.';
      return;
    }

    const status = this.selectedStatusByProduct[product.id];
    const remarks = this.statusRemarkByProduct[product.id] || '';

    if (!status) {
      this.actionError = 'Please select a status first.';
      return;
    }

    this.isUpdatingStatus = true;
    this.actionError = '';
    this.actionMessage = '';

    this.apiService.updateProductStatus(product.id, { status, remarks }).subscribe({
      next: () => {
        product.publishStatus = status;
        this.actionMessage = `Status updated for ${product.sku}.`;
        this.isUpdatingStatus = false;
      },
      error: (err) => {
        this.actionError = err?.error?.message || 'Failed to update status.';
        this.isUpdatingStatus = false;
      }
    });
  }

  getStatusClass(status: string): string {
    return (status || 'Draft').toLowerCase().replace(/\s+/g, '-');
  }

  private normalizeProduct(product: any) {
    return {
      id: product.id ?? product.Id ?? 0,
      sku: product.sku ?? product.Sku ?? 'N/A',
      name: product.name ?? product.Name ?? 'Untitled Product',
      categoryId: product.categoryId ?? product.CategoryId ?? 0,
      publishStatus: product.publishStatus ?? product.PublishStatus ?? 'Draft'
    };
  }
}
