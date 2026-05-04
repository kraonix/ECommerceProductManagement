import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { timeout } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private gatewayUrl = environment.gatewayUrl;
  private readonly requestTimeoutMs = 8000;

  /** Resolves a catalog image path to a full URL using the configured base. */
  imageUrl(path: string | null | undefined): string {
    if (!path) return '';
    // Already absolute (e.g. stored as full URL in older records)
    if (path.startsWith('http://') || path.startsWith('https://')) return path;
    return `${environment.catalogImageBaseUrl}${path}`;
  }

  constructor(private http: HttpClient) {}

  private withTimeout<T>(request$: Observable<T>): Observable<T> {
    return request$.pipe(timeout(this.requestTimeoutMs));
  }

  login(credentials: any): Observable<any> { return this.withTimeout(this.http.post(`${this.gatewayUrl}/auth/login`, credentials)); }
  signup(userData: any): Observable<any> { return this.withTimeout(this.http.post(`${this.gatewayUrl}/auth/signup`, userData)); }
  
  refreshToken(tokenId: string, refreshTokenId: string): Observable<any> {
    return this.withTimeout(this.http.post(`${this.gatewayUrl}/auth/refresh-token`, { token: tokenId, refreshToken: refreshTokenId }));
  }
  forgotPassword(email: string): Observable<any> {
    return this.withTimeout(this.http.post(`${this.gatewayUrl}/auth/forgot-password`, { email }));
  }
  resetPassword(token: string, newPassword: string): Observable<any> {
    return this.withTimeout(this.http.post(`${this.gatewayUrl}/auth/reset-password`, { token, newPassword }));
  }
  changePassword(currentPassword: string, newPassword: string): Observable<any> {
    return this.withTimeout(this.http.post(`${this.gatewayUrl}/auth/change-password`, { currentPassword, newPassword }));
  }
  
  createProduct(payload: any): Observable<any> {
    return this.withTimeout(this.http.post(`${this.gatewayUrl}/catalog/products`, payload));
  }
  updateProduct(productId: number, payload: any): Observable<any> {
    return this.withTimeout(this.http.put(`${this.gatewayUrl}/catalog/products/${productId}`, payload));
  }
  archiveProduct(productId: number, reason: string): Observable<any> {
    return this.withTimeout(this.http.delete(`${this.gatewayUrl}/catalog/products/${productId}`, { body: { reason } }));
  }
  restoreProduct(productId: number): Observable<any> {
    return this.withTimeout(this.http.put(`${this.gatewayUrl}/catalog/products/${productId}/restore`, {}));
  }
  setOutOfStock(productId: number): Observable<any> {
    return this.withTimeout(this.http.put(`${this.gatewayUrl}/catalog/products/${productId}/out-of-stock`, {}));
  }
  getArchivedProducts(): Observable<any> {
    return this.withTimeout(this.http.get(`${this.gatewayUrl}/catalog/products/archived`));
  }

  // ── Search Service ──
  search(params: {
    q?: string; brand?: string; minPrice?: number; maxPrice?: number;
    inStockOnly?: boolean; sortBy?: string; page?: number; pageSize?: number;
  }): Observable<any> {
    const p = new URLSearchParams();
    if (params.q)           p.set('q', params.q);
    if (params.brand)       p.set('brand', params.brand);
    if (params.minPrice != null) p.set('minPrice', String(params.minPrice));
    if (params.maxPrice != null) p.set('maxPrice', String(params.maxPrice));
    if (params.inStockOnly) p.set('inStockOnly', 'true');
    if (params.sortBy)      p.set('sortBy', params.sortBy);
    if (params.page)        p.set('page', String(params.page));
    if (params.pageSize)    p.set('pageSize', String(params.pageSize));
    return this.withTimeout(this.http.get(`${this.gatewayUrl}/search?${p.toString()}`));
  }

  searchSuggest(q: string): Observable<any> {
    return this.withTimeout(this.http.get(`${this.gatewayUrl}/search/suggest?q=${encodeURIComponent(q)}`));
  }
  uploadMedia(productId: number, payload: { fileName: string; base64Content: string }): Observable<any> {
    // Use longer timeout for file uploads (base64 can be large)
    return this.http.post(`${this.gatewayUrl}/catalog/products/${productId}/media`, payload)
      .pipe(timeout(30000));
  }
  deleteMedia(productId: number, mediaId: number): Observable<any> {
    return this.withTimeout(this.http.delete(`${this.gatewayUrl}/catalog/products/${productId}/media/${mediaId}`));
  }
  deleteAllMedia(productId: number): Observable<any> {
    return this.withTimeout(this.http.delete(`${this.gatewayUrl}/catalog/products/${productId}/media`));
  }
  getCategories(): Observable<any> {
    return this.withTimeout(this.http.get(`${this.gatewayUrl}/catalog/categories`));
  }

  getDashboardSummary(): Observable<any> { return this.withTimeout(this.http.get(`${this.gatewayUrl}/admin/reports/dashboard`)); }
  getProducts(): Observable<any> { return this.withTimeout(this.http.get(`${this.gatewayUrl}/catalog/products`)); }
  getProductById(productId: number): Observable<any> { return this.withTimeout(this.http.get(`${this.gatewayUrl}/catalog/products/${productId}`)); }
  getProductAuditHistory(productId: number): Observable<any> { return this.withTimeout(this.http.get(`${this.gatewayUrl}/admin/audit/products/${productId}`)); }
  savePricing(productId: number, payload: { mrp: number; salePrice: number; gst: number }): Observable<any> {
    return this.withTimeout(this.http.put(`${this.gatewayUrl}/workflow/products/${productId}/pricing`, payload));
  }
  saveInventory(productId: number, payload: { availableQty: number; warehouseLocation: string }): Observable<any> {
    return this.withTimeout(this.http.put(`${this.gatewayUrl}/workflow/products/${productId}/inventory`, payload));
  }
  submitForReview(productId: number): Observable<any> {
    return this.withTimeout(this.http.post(`${this.gatewayUrl}/workflow/products/${productId}/submit`, {}));
  }
  updateProductStatus(productId: number, payload: { status: string; remarks: string }): Observable<any> {
    return this.withTimeout(this.http.put(`${this.gatewayUrl}/workflow/products/${productId}/status`, payload));
  }
  
  // New Export Method requiring Blob response
  exportDashboardData(): Observable<Blob> {
    return this.withTimeout(this.http.get(`${this.gatewayUrl}/admin/reports/export`, { responseType: 'blob' }));
  }

  // ── Health ──
  /** Aggregated health across all 5 services. No auth required. */
  getSystemHealth(): Observable<any> {
    // Points directly at the gateway base — not through /gateway prefix
    const gatewayBase = environment.gatewayUrl.replace('/gateway', '');
    return this.http.get(`${gatewayBase}/health/all`).pipe(timeout(10000));
  }
}
