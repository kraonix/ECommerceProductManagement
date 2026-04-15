import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { CartService } from '../../core/services/cart.service';
import { Subject, debounceTime, distinctUntilChanged, takeUntil, switchMap, of } from 'rxjs';

@Component({
  selector: 'app-customer-search',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './customer-search.html',
  styleUrl: './customer-search.scss'
})
export class CustomerSearch implements OnInit, OnDestroy {
  private apiService = inject(ApiService);
  public cartService = inject(CartService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);
  private destroy$ = new Subject<void>();

  // State
  searchTerm = '';
  loading = false;
  error = '';
  userName = localStorage.getItem('user_name') || 'User';
  userMenuOpen = false;
  addedProductIds = new Set<number>();

  // Results
  results: any[] = [];
  totalCount = 0;
  totalPages = 0;
  currentPage = 1;

  // Facets from server
  facets: any = { brands: [], priceRange: { min: 0, max: 9999 }, totalInStock: 0 };

  // Filters
  filters = {
    brand: '',
    minPrice: null as number | null,
    maxPrice: null as number | null,
    inStockOnly: false,
    sortBy: 'relevance'
  };

  // Autocomplete
  suggestions: any[] = [];
  showSuggestions = false;
  private suggestInput$ = new Subject<string>();
  private searchTrigger$ = new Subject<void>();

  ngOnInit(): void {
    // Autocomplete stream
    this.suggestInput$.pipe(
      debounceTime(200),
      distinctUntilChanged(),
      switchMap(q => q.length >= 2 ? this.apiService.searchSuggest(q) : of({ suggestions: [] })),
      takeUntil(this.destroy$)
    ).subscribe((res: any) => {
      this.suggestions = res?.suggestions ?? [];
      this.showSuggestions = this.suggestions.length > 0;
      this.cdr.detectChanges();
    });

    // React to URL query param changes
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const q = params['q'] || '';
      if (q !== this.searchTerm) {
        this.searchTerm = q;
      }
      this.currentPage = Number(params['page'] || 1);
      this.runSearch();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearchInput(): void {
    if (this.searchTerm.length >= 2) {
      this.suggestInput$.next(this.searchTerm);
    } else {
      this.suggestions = [];
      this.showSuggestions = false;
    }
  }

  onSearchSubmit(): void {
    this.showSuggestions = false;
    this.currentPage = 1;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { q: this.searchTerm, page: 1 },
      queryParamsHandling: 'merge'
    });
  }

  selectSuggestion(suggestion: any): void {
    this.searchTerm = suggestion.text;
    this.showSuggestions = false;
    if (suggestion.type === 'product' && suggestion.productId) {
      this.router.navigate(['/customer/product', suggestion.productId]);
    } else {
      this.onSearchSubmit();
    }
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.runSearch();
  }

  clearFilters(): void {
    this.filters = { brand: '', minPrice: null, maxPrice: null, inStockOnly: false, sortBy: 'relevance' };
    this.currentPage = 1;
    this.runSearch();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.runSearch();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  private runSearch(): void {
    this.loading = true;
    this.error = '';
    this.showSuggestions = false;
    this.cdr.detectChanges();

    this.apiService.search({
      q: this.searchTerm || undefined,
      brand: this.filters.brand || undefined,
      minPrice: this.filters.minPrice ?? undefined,
      maxPrice: this.filters.maxPrice ?? undefined,
      inStockOnly: this.filters.inStockOnly,
      sortBy: this.filters.sortBy,
      page: this.currentPage,
      pageSize: 20
    }).subscribe({
      next: (res: any) => {
        this.results = res.results ?? [];
        this.totalCount = res.totalCount ?? 0;
        this.totalPages = res.totalPages ?? 0;
        this.currentPage = res.page ?? 1;
        this.facets = res.facets ?? this.facets;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        if (err?.status === 0) {
          this.error = 'Search service is offline. Make sure SearchService is running on port 5050.';
        } else {
          this.error = 'Search failed. Please try again.';
        }
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  get hasActiveFilters(): boolean {
    return !!(this.filters.brand || this.filters.minPrice != null ||
              this.filters.maxPrice != null || this.filters.inStockOnly ||
              this.filters.sortBy !== 'relevance');
  }

  get pageNumbers(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }

  addToCart(event: Event, product: any): void {
    event.stopPropagation();
    event.preventDefault();
    this.cartService.addToCart(product, 1);
    this.addedProductIds.add(product.id ?? product.productId);
    setTimeout(() => {
      this.addedProductIds.delete(product.id ?? product.productId);
      this.cdr.detectChanges();
    }, 1500);
  }

  toggleUserMenu(): void { this.userMenuOpen = !this.userMenuOpen; }
  closeUserMenu(): void { this.userMenuOpen = false; }
  logout(): void { localStorage.clear(); this.router.navigate(['/auth/login']); }
  goToCart(): void { this.router.navigate(['/customer/cart']); }
  goBack(): void { this.router.navigate(['/customer/products']); }
}
