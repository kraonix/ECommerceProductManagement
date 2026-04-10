import { Injectable, signal, computed } from '@angular/core';

export interface CartItem {
  productId: number;
  name: string;
  price: number;
  quantity: number;
  imageUrl?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly STORAGE_KEY = 'obsidian_ecommerce_cart';
  
  // Reactive state
  public cartItems = signal<CartItem[]>(this.loadCartFromStorage());
  
  // Computed totals
  public cartTotalCount = computed(() => 
    this.cartItems().reduce((acc, item) => acc + item.quantity, 0)
  );

  public cartTotalAmount = computed(() => 
    this.cartItems().reduce((acc, item) => acc + (item.price * item.quantity), 0)
  );

  constructor() {}

  private loadCartFromStorage(): CartItem[] {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      return stored ? JSON.parse(stored) : [];
    } catch {
      return [];
    }
  }

  private saveCartToStorage(items: CartItem[]): void {
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(items));
    this.cartItems.set(items);
  }

  public addToCart(product: any, quantity: number = 1): void {
    const currentItems = [...this.cartItems()];
    const existingIndex = currentItems.findIndex(i => i.productId === product.id);

    if (existingIndex > -1) {
      currentItems[existingIndex].quantity += quantity;
    } else {
      currentItems.push({
        productId: product.id,
        name: product.name,
        price: product.price,
        quantity: quantity,
        // We'll map the first photo or fallback if none
        imageUrl: (product.photos && product.photos.length > 0) ? product.photos[0] : null
      });
    }

    this.saveCartToStorage(currentItems);
  }

  public updateQuantity(productId: number, newQuantity: number): void {
    if (newQuantity <= 0) {
      this.removeFromCart(productId);
      return;
    }
    
    const currentItems = [...this.cartItems()];
    const index = currentItems.findIndex(i => i.productId === productId);
    
    if (index > -1) {
      currentItems[index].quantity = newQuantity;
      this.saveCartToStorage(currentItems);
    }
  }

  public removeFromCart(productId: number): void {
    const currentItems = this.cartItems().filter(i => i.productId !== productId);
    this.saveCartToStorage(currentItems);
  }

  public clearCart(): void {
    this.saveCartToStorage([]);
  }
}
