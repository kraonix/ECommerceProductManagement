import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-customer-cart',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './customer-cart.html',
  styleUrl: './customer-cart.scss'
})
export class CustomerCartComponent {
  public cartService = inject(CartService);
  private router = inject(Router);

  get items() {
    return this.cartService.cartItems();
  }

  get totalAmount() {
    return this.cartService.cartTotalAmount();
  }

  incrementQuantity(productId: number, currentQty: number): void {
    this.cartService.updateQuantity(productId, currentQty + 1);
  }

  decrementQuantity(productId: number, currentQty: number): void {
    this.cartService.updateQuantity(productId, currentQty - 1);
  }

  removeItem(productId: number): void {
    this.cartService.removeFromCart(productId);
  }

  goBack(): void {
    this.router.navigate(['/customer/products']);
  }

  checkout(): void {
    if (this.items.length === 0) return;
    alert('Processing checkout using Obsidian Enterprise Payment Gateway (Mock)');
    this.cartService.clearCart();
    this.router.navigate(['/customer/products']);
  }
}
