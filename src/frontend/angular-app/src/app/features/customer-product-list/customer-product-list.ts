import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-customer-product-list',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './customer-product-list.html',
  styleUrl: './customer-product-list.scss'
})
export class CustomerProductList implements OnInit {
  private apiService = inject(ApiService);

  products: any[] = [];
  filteredProducts: any[] = [];
  searchTerm = '';
  loading = true;
  error = '';

  ngOnInit(): void {
    this.apiService.getProducts().subscribe({
      next: (data: any) => {
        const products = Array.isArray(data) ? data : data?.items || data?.data || [];
        this.products = products.map((p: any) => ({
          id: p.id ?? p.Id ?? 0,
          name: p.name ?? p.Name ?? 'Untitled Product',
          sku: p.sku ?? p.Sku ?? 'N/A',
          publishStatus: p.publishStatus ?? p.PublishStatus ?? 'Draft'
        }));
        this.filteredProducts = [...this.products];
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load products.';
        this.loading = false;
      }
    });
  }

  filter(): void {
    const term = this.searchTerm.trim().toLowerCase();
    this.filteredProducts = this.products.filter((p) =>
      `${p.name} ${p.sku}`.toLowerCase().includes(term)
    );
  }
}
