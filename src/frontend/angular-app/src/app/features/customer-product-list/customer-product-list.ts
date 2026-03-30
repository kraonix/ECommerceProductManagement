import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-customer-product-list',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './customer-product-list.html',
  styleUrl: './customer-product-list.scss'
})
export class CustomerProductList implements OnInit, OnDestroy {
  private apiService = inject(ApiService);
  private cdr = inject(ChangeDetectorRef);

  products: any[] = [];
  filteredProducts: any[] = [];
  searchTerm = '';
  loading = true;
  error = '';
  
  // Carousel State
  currentSlide = 0;
  carouselSlides = [
    { title: 'Spring Essentials', subtitle: 'Refresh your home with green deals', bgColor: '#e8f5ed', imageUrl: '' },
    { title: 'Tech Gadgets', subtitle: 'Upgrade your productivity today', bgColor: '#f4f8f4', imageUrl: '' },
    { title: 'Fashion Clearance', subtitle: 'Up to 50% off selected styles', bgColor: '#eef4fb', imageUrl: '' }
  ];
  private sliderInterval: any;

  ngOnInit(): void {
    this.apiService.getProducts().subscribe({
      next: (data: any) => {
        const products = Array.isArray(data) ? data : data?.items || data?.data || [];
        this.products = products.map((p: any) => ({
          id: p.id ?? p.Id ?? 0,
          name: p.name ?? p.Name ?? 'Untitled Product',
          sku: p.sku ?? p.Sku ?? 'N/A',
          brand: p.brand ?? p.Brand ?? 'Unknown Brand',
          description: p.description ?? p.Description ?? '',
          publishStatus: p.publishStatus ?? p.PublishStatus ?? 'Draft'
        }));
        
        // Removed publish filter so user sees all their DB data.
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
  
  addToCart(event: Event, product: any): void {
    event.stopPropagation();
    event.preventDefault();
    alert('Added ' + product.name + ' to cart!');
  }
}
