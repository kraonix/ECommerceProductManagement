import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-customer-product-detail',
  imports: [CommonModule, RouterModule],
  templateUrl: './customer-product-detail.html',
  styleUrl: './customer-product-detail.scss'
})
export class CustomerProductDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private apiService = inject(ApiService);
  private cartService = inject(CartService);
  private cdr = inject(ChangeDetectorRef);

  loading = true;
  error = '';
  product: any = null;
  addedFeedback = false;
  selectedPhotoIndex = 0;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    const productId = Number.isFinite(id) && id > 0 ? id : 1;

    this.apiService.getProductById(productId).subscribe({
      next: (data: any) => {
        this.product = {
          id: data?.productId ?? data?.ProductId ?? data?.id ?? productId,
          name: data?.name ?? data?.Name ?? 'Untitled Product',
          sku: data?.sku ?? data?.Sku ?? 'N/A',
          brand: data?.brand ?? data?.Brand ?? 'Obsidian Standard',
          price: data?.price ?? data?.Price ?? 0,
          stockQuantity: data?.stockQuantity ?? data?.StockQuantity ?? 0,
          weightKg: data?.weightKg ?? data?.WeightKg ?? 0,
          dimensionsCm: data?.dimensionsCm ?? data?.DimensionsCm ?? '',
          material: data?.material ?? data?.Material ?? '',
          color: data?.color ?? data?.Color ?? '',
          warrantyPeriod: data?.warrantyPeriod ?? data?.WarrantyPeriod ?? '',
          manufacturer: data?.manufacturer ?? data?.Manufacturer ?? '',
          highlights: data?.highlights ?? data?.Highlights ?? '',
          hardwareInterface: data?.hardwareInterface ?? data?.HardwareInterface ?? '',
          description: data?.description ?? data?.Description ?? 'No description available.',
          publishStatus: data?.publishStatus ?? data?.PublishStatus ?? 'Draft',
          photos: data?.photos ?? data?.Photos ?? []
        };
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        if (err?.name === 'TimeoutError') {
          this.error = 'Request timed out. Make sure the gateway and catalog service are running.';
        } else if (err?.status === 0) {
          this.error = 'Cannot reach the server. Make sure all services are running via run-all.bat.';
        } else if (err?.status === 401) {
          this.error = 'Session expired. Please log in again.';
        } else if (err?.status === 404) {
          this.error = 'Product not found.';
        } else {
          this.error = `Failed to load product (${err?.status ?? 'unknown error'}).`;
        }
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  addToCart(): void {
    if (this.product) {
      this.cartService.addToCart(this.product, 1);
      this.addedFeedback = true;
      setTimeout(() => {
        this.addedFeedback = false;
        this.cdr.detectChanges();
      }, 2000);
    }
  }
}
