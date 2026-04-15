import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-product-wizard',
  imports: [CommonModule, FormsModule],
  templateUrl: './product-wizard.html',
  styleUrl: './product-wizard.scss',
})
export class ProductWizard implements OnInit {
  private apiService = inject(ApiService);
  public router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  currentStep = 1;

  // Null until the product is actually created in step 1
  productId: number | null = null;

  categories: any[] = [];

  // Step 1 — Basic Info
  basicInfo = {
    categoryId: 0,
    sku: '',
    name: '',
    brand: '',
    description: '',
    price: 0,
    stockQuantity: 0
  };

  // Step 2 — Media
  mediaFile = { fileName: '', base64Content: '' };

  // Step 3 — Pricing
  pricing = { mrp: 0, salePrice: 0, gst: 18 };

  // Step 4 — Inventory
  inventory = { availableQty: 0, warehouseLocation: 'WH-A1' };

  busy = false;
  message = '';
  error = '';

  readonly steps = ['Basic Info', 'Media', 'Pricing', 'Inventory', 'Review', 'Publish'];

  ngOnInit(): void {
    this.apiService.getCategories().subscribe({
      next: (data: any) => {
        this.categories = Array.isArray(data) ? data : data?.items || data?.data || [];
        if (this.categories.length > 0) {
          this.basicInfo.categoryId = this.categories[0].categoryId ?? this.categories[0].CategoryId ?? 0;
        }
        this.cdr.detectChanges();
      },
      error: () => {
        // Categories failed to load — user can still type a category ID
        this.cdr.detectChanges();
      }
    });
  }

  setStep(step: number) {
    if (step < 1 || step > 6) return;
    // Prevent skipping ahead past step 1 if product hasn't been created yet
    if (step > 1 && this.productId === null) {
      this.error = 'Please save Basic Info first to create the product.';
      return;
    }
    this.currentStep = step;
    this.message = '';
    this.error = '';
  }

  // Step 1: Create the product and advance
  saveBasicInfo(): void {
    if (!this.basicInfo.name.trim() || !this.basicInfo.sku.trim()) {
      this.error = 'Product name and SKU are required.';
      return;
    }

    this.busy = true;
    this.error = '';
    this.message = '';

    const payload = { ...this.basicInfo };

    if (this.productId === null) {
      // Create new product
      this.apiService.createProduct(payload).subscribe({
        next: (res: any) => {
          this.productId = res.productId ?? res.ProductId ?? res.id ?? null;
          this.busy = false;
          this.message = `Product created (ID: ${this.productId}). Proceed to next step.`;
          this.cdr.detectChanges();
        },
        error: (err: any) => {
          this.busy = false;
          this.error = err?.error?.message || 'Failed to create product.';
          this.cdr.detectChanges();
        }
      });
    } else {
      // Update existing product
      this.apiService.updateProduct(this.productId, payload).subscribe({
        next: () => {
          this.busy = false;
          this.message = 'Basic info updated.';
          this.cdr.detectChanges();
        },
        error: (err: any) => {
          this.busy = false;
          this.error = err?.error?.message || 'Failed to update product.';
          this.cdr.detectChanges();
        }
      });
    }
  }

  // Step 2: Upload media
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.mediaFile.fileName = file.name;
    const reader = new FileReader();
    reader.onload = () => {
      const result = reader.result as string;
      // Strip the data URL prefix (e.g. "data:image/png;base64,")
      this.mediaFile.base64Content = result.split(',')[1] ?? '';
      this.cdr.detectChanges();
    };
    reader.readAsDataURL(file);
  }

  uploadMedia(): void {
    if (!this.productId) { this.error = 'Create the product first.'; return; }
    if (!this.mediaFile.fileName) { this.error = 'Select a file to upload.'; return; }

    this.busy = true;
    this.error = '';
    this.message = '';

    this.apiService.uploadMedia(this.productId, this.mediaFile).subscribe({
      next: () => {
        this.busy = false;
        this.message = 'Media uploaded successfully.';
        this.mediaFile = { fileName: '', base64Content: '' };
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.busy = false;
        this.error = err?.error?.message || 'Failed to upload media. Only .jpg, .jpeg, .png are allowed.';
        this.cdr.detectChanges();
      }
    });
  }

  // Step 3: Save pricing
  savePricing(): void {
    if (!this.productId) { this.error = 'Create the product first.'; return; }

    this.busy = true;
    this.error = '';
    this.message = '';

    this.apiService.savePricing(this.productId, this.pricing).subscribe({
      next: () => {
        this.busy = false;
        this.message = 'Pricing saved successfully.';
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.busy = false;
        this.error = err?.error?.message || 'Failed to save pricing.';
        this.cdr.detectChanges();
      }
    });
  }

  // Step 4: Save inventory
  saveInventory(): void {
    if (!this.productId) { this.error = 'Create the product first.'; return; }

    this.busy = true;
    this.error = '';
    this.message = '';

    this.apiService.saveInventory(this.productId, this.inventory).subscribe({
      next: () => {
        this.busy = false;
        this.message = 'Inventory saved successfully.';
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.busy = false;
        this.error = err?.error?.message || 'Failed to save inventory.';
        this.cdr.detectChanges();
      }
    });
  }

  // Step 5/6: Submit for review
  submitForReview(): void {
    if (!this.productId) { this.error = 'Create the product first.'; return; }

    this.busy = true;
    this.error = '';
    this.message = '';

    this.apiService.submitForReview(this.productId).subscribe({
      next: () => {
        this.busy = false;
        this.message = 'Product submitted for review. An admin will approve it shortly.';
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.busy = false;
        this.error = err?.error?.message || 'Failed to submit product for review.';
        this.cdr.detectChanges();
      }
    });
  }

  nextStep(): void {
    if (this.currentStep === 1 && this.productId === null) {
      this.error = 'Save Basic Info first before proceeding.';
      return;
    }
    this.setStep(this.currentStep + 1);
  }

  prevStep(): void {
    this.setStep(this.currentStep - 1);
  }

  get readinessScore(): number {
    return Math.round((this.currentStep / 6) * 100);
  }

  get warnings(): string[] {
    const list: string[] = [];
    if (!this.productId) list.push('Product not created yet. Save Basic Info first.');
    if (this.pricing.salePrice <= 0) list.push('Sale price is missing or invalid.');
    if (this.pricing.mrp > 0 && this.pricing.salePrice > this.pricing.mrp) list.push('Sale price cannot be higher than MRP.');
    if (this.inventory.availableQty <= 0) list.push('Inventory quantity is not set.');
    if (this.currentStep < 5) list.push('Complete all wizard steps before submitting.');
    return list;
  }
}
