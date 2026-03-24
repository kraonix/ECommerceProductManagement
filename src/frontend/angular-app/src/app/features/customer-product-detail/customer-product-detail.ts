import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-customer-product-detail',
  imports: [CommonModule, RouterModule],
  templateUrl: './customer-product-detail.html',
  styleUrl: './customer-product-detail.scss'
})
export class CustomerProductDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private apiService = inject(ApiService);

  loading = true;
  error = '';
  product: any = null;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    const productId = Number.isFinite(id) && id > 0 ? id : 1;

    this.apiService.getProductById(productId).subscribe({
      next: (data: any) => {
        this.product = {
          id: data?.id ?? data?.Id ?? productId,
          name: data?.name ?? data?.Name ?? 'Untitled Product',
          sku: data?.sku ?? data?.Sku ?? 'N/A',
          description: data?.description ?? data?.Description ?? 'No description available.',
          publishStatus: data?.publishStatus ?? data?.PublishStatus ?? 'Draft',
          mrp: data?.mrp ?? data?.MRP ?? 0,
          salePrice: data?.salePrice ?? data?.SalePrice ?? 0
        };
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load product details.';
        this.loading = false;
      }
    });
  }
}
