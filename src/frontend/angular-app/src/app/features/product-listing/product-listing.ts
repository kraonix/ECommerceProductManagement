import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
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

  // Edit drawer state
  editDrawerProduct: any = null;
  editForm: any = {};
  editPhotos: any[] = []; // { mediaId, url }
  mediaFile = { fileName: '', base64Content: '' };
  isSavingEdit = false;
  isUploadingMedia = false;
  isDeletingMedia = false;
  editMessage = '';
  editError = '';

  private apiService = inject(ApiService);
  private cdr = inject(ChangeDetectorRef);

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
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.loadError = err?.status === 401
          ? 'Unauthorized request. Please login again.'
          : 'Unable to load catalog data. Verify gateway and catalog service are running.';
        this.isLoading = false;
        this.cdr.detectChanges();
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
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.actionError = err?.error?.message || 'Failed to update status.';
        this.isUpdatingStatus = false;
        this.cdr.detectChanges();
      }
    });
  }

  getStatusClass(status: string): string {
    return (status || 'Draft').toLowerCase().replace(/\s+/g, '-');
  }

  openEditDrawer(product: any): void {
    // Load full product details including media
    this.apiService.getProductById(product.id).subscribe({
      next: (data: any) => {
        this.editDrawerProduct = product;
        this.editForm = {
          categoryId: data.categoryId ?? data.CategoryId ?? product.categoryId,
          sku: data.sku ?? data.Sku ?? product.sku,
          name: data.name ?? data.Name ?? product.name,
          brand: data.brand ?? data.Brand ?? '',
          description: data.description ?? data.Description ?? '',
          price: data.price ?? data.Price ?? 0,
          stockQuantity: data.stockQuantity ?? data.StockQuantity ?? 0,
          weightKg: data.weightKg ?? data.WeightKg ?? 0,
          dimensionsCm: data.dimensionsCm ?? data.DimensionsCm ?? '',
          material: data.material ?? data.Material ?? '',
          color: data.color ?? data.Color ?? '',
          warrantyPeriod: data.warrantyPeriod ?? data.WarrantyPeriod ?? '',
          manufacturer: data.manufacturer ?? data.Manufacturer ?? '',
          highlights: data.highlights ?? data.Highlights ?? '',
          hardwareInterface: data.hardwareInterface ?? data.HardwareInterface ?? ''
        };
        this.editPhotos = (data.mediaAssets ?? data.MediaAssets ?? []).map((m: any) => ({
          mediaId: m.mediaId ?? m.MediaId,
          url: m.url ?? m.Url
        }));
        this.mediaFile = { fileName: '', base64Content: '' };
        this.editMessage = '';
        this.editError = '';
        this.cdr.detectChanges();
      },
      error: () => {
        // Fallback to basic info if full load fails
        this.editDrawerProduct = product;
        this.editForm = { ...product };
        this.editPhotos = [];
        this.editMessage = '';
        this.editError = '';
        this.cdr.detectChanges();
      }
    });
  }

  closeEditDrawer(): void {
    this.editDrawerProduct = null;
    this.cdr.detectChanges();
  }

  saveEdit(): void {
    if (!this.editDrawerProduct) return;
    this.isSavingEdit = true;
    this.editMessage = '';
    this.editError = '';

    this.apiService.updateProduct(this.editDrawerProduct.id, this.editForm).subscribe({
      next: () => {
        this.isSavingEdit = false;
        this.editMessage = 'Product updated successfully.';
        // Update the table row
        const idx = this.products.findIndex(p => p.id === this.editDrawerProduct.id);
        if (idx > -1) {
          this.products[idx] = { ...this.products[idx], name: this.editForm.name, sku: this.editForm.sku };
          this.filteredProducts = [...this.products];
        }
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.isSavingEdit = false;
        this.editError = err?.error?.message || 'Failed to save changes.';
        this.cdr.detectChanges();
      }
    });
  }

  onMediaFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.mediaFile.fileName = file.name;
    const reader = new FileReader();
    reader.onload = () => {
      this.mediaFile.base64Content = (reader.result as string).split(',')[1] ?? '';
      this.cdr.detectChanges();
    };
    reader.readAsDataURL(file);
  }

  uploadMedia(): void {
    if (!this.editDrawerProduct || !this.mediaFile.fileName) return;
    this.isUploadingMedia = true;
    this.editMessage = '';
    this.editError = '';

    this.apiService.uploadMedia(this.editDrawerProduct.id, this.mediaFile).subscribe({
      next: () => {
        this.isUploadingMedia = false;
        this.editMessage = 'Image uploaded.';
        this.mediaFile = { fileName: '', base64Content: '' };
        // Reload photos
        this.reloadPhotos();
      },
      error: (err: any) => {
        this.isUploadingMedia = false;
        this.editError = err?.error?.message || 'Upload failed. Only .jpg, .jpeg, .png allowed.';
        this.cdr.detectChanges();
      }
    });
  }

  deletePhoto(mediaId: number): void {
    if (!this.editDrawerProduct) return;
    this.isDeletingMedia = true;
    this.editMessage = '';
    this.editError = '';

    this.apiService.deleteMedia(this.editDrawerProduct.id, mediaId).subscribe({
      next: () => {
        this.isDeletingMedia = false;
        this.editPhotos = this.editPhotos.filter(p => p.mediaId !== mediaId);
        this.editMessage = 'Image deleted.';
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.isDeletingMedia = false;
        this.editError = err?.error?.message || 'Failed to delete image.';
        this.cdr.detectChanges();
      }
    });
  }

  deleteAllPhotos(): void {
    if (!this.editDrawerProduct || this.editPhotos.length === 0) return;
    this.isDeletingMedia = true;
    this.editMessage = '';
    this.editError = '';

    this.apiService.deleteAllMedia(this.editDrawerProduct.id).subscribe({
      next: () => {
        this.isDeletingMedia = false;
        this.editPhotos = [];
        this.editMessage = 'All images deleted.';
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.isDeletingMedia = false;
        this.editError = err?.error?.message || 'Failed to delete images.';
        this.cdr.detectChanges();
      }
    });
  }

  private reloadPhotos(): void {
    this.apiService.getProductById(this.editDrawerProduct.id).subscribe({
      next: (data: any) => {
        this.editPhotos = (data.mediaAssets ?? data.MediaAssets ?? []).map((m: any) => ({
          mediaId: m.mediaId ?? m.MediaId,
          url: m.url ?? m.Url
        }));
        this.cdr.detectChanges();
      }
    });
  }

  private normalizeProduct(product: any) {
    return {
      id: product.productId ?? product.ProductId ?? product.id ?? product.Id ?? 0,
      sku: product.sku ?? product.Sku ?? 'N/A',
      name: product.name ?? product.Name ?? 'Untitled Product',
      categoryId: product.categoryId ?? product.CategoryId ?? 0,
      publishStatus: product.publishStatus ?? product.PublishStatus ?? 'Draft',
      photos: product.photos ?? product.Photos ?? []
    };
  }
}
