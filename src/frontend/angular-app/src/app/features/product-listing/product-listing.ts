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

  // Tab state
  activeTab: 'active' | 'archived' = 'active';
  archivedProducts: any[] = [];
  filteredArchived: any[] = [];
  isLoadingArchived = false;
  archivedError = '';
  isRestoringProduct = false;

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

  // Delete flow state
  showDeleteChoiceModal = false;   // Step 1: "Out of Stock" vs "Delete"
  showDeleteConfirmModal = false;  // Step 2: type CONFIRM
  deleteConfirmInput = '';
  isDeletingProduct = false;
  deleteProductError = '';

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
      p.name.toLowerCase().includes(term) || p.sku.toLowerCase().includes(term)
    );
    if (this.activeTab === 'archived') {
      this.filterArchived();
    }
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

  switchTab(tab: 'active' | 'archived'): void {
    this.activeTab = tab;
    this.actionMessage = '';
    this.actionError = '';

    if (tab === 'archived' && this.archivedProducts.length === 0) {
      this.loadArchivedProducts();
    }
    this.cdr.detectChanges();
  }

  loadArchivedProducts(): void {
    this.isLoadingArchived = true;
    this.archivedError = '';

    this.apiService.getArchivedProducts().subscribe({
      next: (data: any) => {
        const products = Array.isArray(data) ? data : data?.items || data?.data || [];
        this.archivedProducts = products.map((p: any) => this.normalizeArchivedProduct(p));
        this.filteredArchived = [...this.archivedProducts];
        this.isLoadingArchived = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.archivedError = 'Failed to load archived products.';
        this.isLoadingArchived = false;
        this.cdr.detectChanges();
      }
    });
  }

  filterArchived(): void {
    const term = this.searchTerm.toLowerCase().trim();
    this.filteredArchived = this.archivedProducts.filter(p =>
      p.name.toLowerCase().includes(term) || p.sku.toLowerCase().includes(term)
    );
  }

  restoreProduct(product: any): void {
    this.isRestoringProduct = true;
    this.actionMessage = '';
    this.actionError = '';

    this.apiService.restoreProduct(product.id).subscribe({
      next: () => {
        this.isRestoringProduct = false;
        // Remove from archived list
        this.archivedProducts = this.archivedProducts.filter(p => p.id !== product.id);
        this.filteredArchived = this.filteredArchived.filter(p => p.id !== product.id);
        this.actionMessage = `"${product.name}" restored to Draft.`;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.isRestoringProduct = false;
        this.actionError = err?.error?.message || 'Failed to restore product.';
        this.cdr.detectChanges();
      }
    });
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

  // ── Delete flow ──

  openDeleteChoiceModal(): void {
    this.showDeleteChoiceModal = true;
    this.deleteProductError = '';
    this.cdr.detectChanges();
  }

  closeDeleteModals(): void {
    this.showDeleteChoiceModal = false;
    this.showDeleteConfirmModal = false;
    this.deleteConfirmInput = '';
    this.deleteProductError = '';
    this.cdr.detectChanges();
  }

  markOutOfStock(): void {
    if (!this.editDrawerProduct) return;
    this.isDeletingProduct = true;
    this.deleteProductError = '';

    this.apiService.setOutOfStock(this.editDrawerProduct.id).subscribe({
      next: () => {
        this.isDeletingProduct = false;
        this.editForm.stockQuantity = 0;
        this.closeDeleteModals();
        this.editMessage = `"${this.editDrawerProduct.name}" is now out of stock.`;
        // Update table row
        const idx = this.products.findIndex(p => p.id === this.editDrawerProduct.id);
        if (idx > -1) this.products[idx] = { ...this.products[idx] };
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.isDeletingProduct = false;
        this.deleteProductError = err?.error?.message || 'Failed to update stock.';
        this.cdr.detectChanges();
      }
    });
  }

  proceedToDeleteConfirm(): void {
    this.showDeleteChoiceModal = false;
    this.showDeleteConfirmModal = true;
    this.deleteConfirmInput = '';
    this.deleteProductError = '';
    this.cdr.detectChanges();
  }

  confirmDelete(): void {
    if (this.deleteConfirmInput !== 'CONFIRM') {
      this.deleteProductError = 'Please type CONFIRM exactly to proceed.';
      this.cdr.detectChanges();
      return;
    }
    if (!this.editDrawerProduct) return;

    this.isDeletingProduct = true;
    this.deleteProductError = '';

    this.apiService.archiveProduct(this.editDrawerProduct.id, `Deleted by admin`).subscribe({
      next: () => {
        this.isDeletingProduct = false;
        // Remove from active list
        this.products = this.products.filter(p => p.id !== this.editDrawerProduct.id);
        this.filteredProducts = this.filteredProducts.filter(p => p.id !== this.editDrawerProduct.id);
        this.closeDeleteModals();
        this.editDrawerProduct = null;
        this.actionMessage = 'Product archived successfully.';
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.isDeletingProduct = false;
        this.deleteProductError = err?.error?.message || 'Failed to delete product.';
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

  private normalizeArchivedProduct(product: any) {
    return {
      id: product.productId ?? product.ProductId ?? product.id ?? product.Id ?? 0,
      sku: product.sku ?? product.Sku ?? 'N/A',
      name: product.name ?? product.Name ?? 'Untitled Product',
      categoryId: product.categoryId ?? product.CategoryId ?? 0,
      publishStatus: product.publishStatus ?? product.PublishStatus ?? 'Archived',
      photos: product.photos ?? product.Photos ?? [],
      archivedAt: product.archivedAt ?? product.ArchivedAt ?? null,
      archivedBy: product.archivedBy ?? product.ArchivedBy ?? 'Unknown',
      archivedReason: product.archivedReason ?? product.ArchivedReason ?? ''
    };
  }
}
