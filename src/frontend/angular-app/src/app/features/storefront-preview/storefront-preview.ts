import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-storefront-preview',
  imports: [CommonModule],
  templateUrl: './storefront-preview.html',
  styleUrl: './storefront-preview.scss',
})
export class StorefrontPreview implements OnInit {
  private route = inject(ActivatedRoute);
  private apiService = inject(ApiService);
  private cdr = inject(ChangeDetectorRef);

  product: any = null;
  relatedProducts: any[] = [];
  productId = 1;
  loading = true;
  error = '';

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const parsedId = Number(idParam);
    this.productId = Number.isFinite(parsedId) && parsedId > 0 ? parsedId : 1;
    this.loadPreview();
  }

  private loadPreview(): void {
    this.loading = true;
    this.error = '';

    forkJoin({
      product: this.apiService.getProductById(this.productId).pipe(catchError(() => of(null))),
      products: this.apiService.getProducts().pipe(catchError(() => of([])))
    }).subscribe({
      next: ({ product, products }) => {
        this.product = this.normalizeProduct(product);
        this.relatedProducts = this.normalizeProducts(products)
          .filter((p: any) => p.id !== this.productId)
          .slice(0, 4);
        const noProduct = !product;
        const noProducts = this.relatedProducts.length === 0;
        if (noProduct && noProducts) {
          this.error = 'Unable to load storefront data. Verify gateway and catalog service are running.';
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = 'Failed to load storefront preview.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  private normalizeProducts(data: any): any[] {
    const products = Array.isArray(data) ? data : data?.items || data?.data || [];
    return products.map((p: any) => this.normalizeProduct(p)).filter((p: any) => p.id > 0);
  }

  private normalizeProduct(product: any): any {
    if (!product) {
      return {
        id: this.productId,
        name: 'Preview Product',
        sku: 'N/A',
        description: 'Product details are currently unavailable.',
        mrp: 0,
        salePrice: 0,
        availableQty: 0,
        publishStatus: 'Draft'
      };
    }

    return {
      id: product.productId ?? product.ProductId ?? product.id ?? product.Id ?? this.productId,
      name: product.name ?? product.Name ?? 'Untitled Product',
      sku: product.sku ?? product.Sku ?? 'N/A',
      brand: product.brand ?? product.Brand ?? 'Unknown Brand',
      description: product.description ?? product.Description ?? 'No description available.',
      publishStatus: product.publishStatus ?? product.PublishStatus ?? 'Draft'
    };
  }
}
