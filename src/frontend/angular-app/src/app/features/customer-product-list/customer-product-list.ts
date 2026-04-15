import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-customer-product-list',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './customer-product-list.html',
  styleUrl: './customer-product-list.scss'
})
export class CustomerProductList implements OnInit, OnDestroy {
  private apiService = inject(ApiService);
  public cartService = inject(CartService); // public for template binding
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);

  products: any[] = [];
  filteredProducts: any[] = [];
  searchTerm = '';
  loading = true;
  error = '';
  userName = localStorage.getItem('user_name') || 'User';
  userRole = localStorage.getItem('user_role') || 'Customer';
  userMenuOpen = false;
  addedProductIds = new Set<number>();

  // Carousel State
  currentSlide = 0;
  carouselSlides = [
    { title: 'Flux Core Alpha', subtitle: 'Enterprise Performance', bgColor: '#1c1b1b' },
    { title: 'Obsidian Chrono', subtitle: 'Analog Precision', bgColor: '#201f1f' },
    { title: 'Aura Sound Shell', subtitle: 'Noise Isolation', bgColor: '#1a1a1e' }
  ];
  private sliderInterval: any;

  ngOnInit(): void {
    this.apiService.getProducts().subscribe({
      next: (data: any) => {
        const products = Array.isArray(data) ? data : data?.items || data?.data || [];
        this.products = products.map((p: any) => ({
          id: p.productId ?? p.ProductId ?? p.id ?? p.Id ?? 0,
          name: p.name ?? p.Name ?? 'Untitled Product',
          sku: p.sku ?? p.Sku ?? 'N/A',
          brand: p.brand ?? p.Brand ?? 'Unknown Brand',
          price: p.price ?? p.Price ?? 0,
          weightKg: p.weightKg ?? p.WeightKg ?? 0,
          dimensionsCm: p.dimensionsCm ?? p.DimensionsCm ?? '',
          material: p.material ?? p.Material ?? '',
          color: p.color ?? p.Color ?? '',
          warrantyPeriod: p.warrantyPeriod ?? p.WarrantyPeriod ?? '',
          manufacturer: p.manufacturer ?? p.Manufacturer ?? '',
          highlights: p.highlights ?? p.Highlights ?? '',
          hardwareInterface: p.hardwareInterface ?? p.HardwareInterface ?? '',
          description: p.description ?? p.Description ?? '',
          publishStatus: p.publishStatus ?? p.PublishStatus ?? 'Draft',
          photos: p.photos ?? p.Photos ?? []
        }));
        
        // Only show Published products to customers
        this.products = this.products.filter(p => p.publishStatus === 'Published');
        this.filteredProducts = [...this.products];
        this.loading = false;
        this.cdr.detectChanges();
        
        this.startCarousel();
      },
      error: () => {
        this.error = 'Failed to load products.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  ngOnDestroy(): void {
    if (this.sliderInterval) clearInterval(this.sliderInterval);
  }

  filter(): void {
    const term = this.searchTerm.trim().toLowerCase();
    this.filteredProducts = this.products.filter((p) =>
      `${p.name} ${p.sku}`.toLowerCase().includes(term)
    );
  }
  
  startCarousel(): void {
    this.sliderInterval = setInterval(() => {
      this.nextSlide();
    }, 5000);
  }
  
  nextSlide(): void {
    this.currentSlide = (this.currentSlide + 1) % this.carouselSlides.length;
  }
  
  prevSlide(): void {
    this.currentSlide = (this.currentSlide - 1 + this.carouselSlides.length) % this.carouselSlides.length;
  }
  
  toggleUserMenu(): void {
    this.userMenuOpen = !this.userMenuOpen;
  }

  closeUserMenu(): void {
    this.userMenuOpen = false;
  }

  logout(): void {
    this.userMenuOpen = false;
    localStorage.clear();
    this.router.navigate(['/auth/login']);
  }

  addToCart(event: Event, product: any): void {
    event.stopPropagation();
    event.preventDefault();

    this.cartService.addToCart(product, 1);
    
    // UI Feedback
    this.addedProductIds.add(product.id);
    setTimeout(() => {
      this.addedProductIds.delete(product.id);
      this.cdr.detectChanges();
    }, 1500);
  }

  goToCart(): void {
    this.router.navigate(['/customer/cart']);
  }
}
