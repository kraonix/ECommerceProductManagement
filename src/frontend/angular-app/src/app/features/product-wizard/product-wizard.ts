import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { inject } from '@angular/core';

@Component({
  selector: 'app-product-wizard',
  imports: [CommonModule, FormsModule],
  templateUrl: './product-wizard.html',
  styleUrl: './product-wizard.scss',
})
export class ProductWizard {
  private apiService = inject(ApiService);

  currentStep = 1;
  productId = 1;
  pricing = { mrp: 0, salePrice: 0, gst: 18 };
  inventory = { availableQty: 0, warehouseLocation: 'WH-A1' };
  busy = false;
  message = '';
  error = '';

  readonly steps = [
    'Basic Info',
    'Media',
    'Pricing',
    'Inventory',
    'Review',
    'Publish'
  ];
  
  setStep(step: number) {
    if (step < 1 || step > 6) return;
    this.currentStep = step;
    this.message = '';
    this.error = '';
  }

  nextStep() {
    this.setStep(this.currentStep + 1);
  }

  prevStep() {
    this.setStep(this.currentStep - 1);
  }

  savePricing() {
    this.busy = true;
    this.error = '';
    this.message = '';

    this.apiService.savePricing(this.productId, this.pricing).subscribe({
      next: () => {
        this.busy = false;
        this.message = 'Pricing saved successfully.';
      },
      error: (err) => {
        this.busy = false;
        this.error = err?.error?.message || 'Failed to save pricing.';
      }
    });
  }

  saveInventory() {
    this.busy = true;
    this.error = '';
    this.message = '';

    this.apiService.saveInventory(this.productId, this.inventory).subscribe({
      next: () => {
        this.busy = false;
        this.message = 'Inventory saved successfully.';
      },
      error: (err) => {
        this.busy = false;
        this.error = err?.error?.message || 'Failed to save inventory.';
      }
    });
  }

  submitForReview() {
    this.busy = true;
    this.error = '';
    this.message = '';
    this.apiService.submitForReview(this.productId).subscribe({
      next: () => {
        this.busy = false;
        this.message = 'Submitted for review successfully.';
      },
      error: (err) => {
        this.busy = false;
        this.error = err?.error?.message || 'Failed to submit product for review.';
      }
    });
  }

  get readinessScore(): number {
    return Math.round((this.currentStep / 6) * 100);
  }

  get warnings(): string[] {
    const list: string[] = [];
    if (this.pricing.salePrice <= 0) list.push('Sale price is missing or invalid.');
    if (this.pricing.mrp > 0 && this.pricing.salePrice > this.pricing.mrp) list.push('Sale price cannot be higher than MRP.');
    if (this.inventory.availableQty <= 0) list.push('Inventory quantity is not set.');
    if (this.currentStep < 5) list.push('Complete all wizard steps before publish.');
    return list;
  }
}
