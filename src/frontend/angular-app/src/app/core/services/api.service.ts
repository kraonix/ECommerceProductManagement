import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { timeout } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private gatewayUrl = 'http://localhost:5000/gateway';
  private readonly requestTimeoutMs = 8000;

  constructor(private http: HttpClient) {}

  private withTimeout<T>(request$: Observable<T>): Observable<T> {
    return request$.pipe(timeout(this.requestTimeoutMs));
  }

  login(credentials: any): Observable<any> { return this.withTimeout(this.http.post(`${this.gatewayUrl}/auth/login`, credentials)); }
  signup(userData: any): Observable<any> { return this.withTimeout(this.http.post(`${this.gatewayUrl}/auth/signup`, userData)); }
  
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
}
