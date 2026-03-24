import { Component, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  isLoginMode = true;
  isLoading = false;
  errorMessage = '';

  loginData = { email: '', password: '' };
  signupData = { fullName: '', email: '', password: '', role: 'Admin' };

  private apiService = inject(ApiService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  toggleMode(form: NgForm) {
    this.isLoginMode = !this.isLoginMode;
    this.errorMessage = '';
    form.resetForm();
    if (!this.isLoginMode) this.signupData.role = 'Admin';
    this.cdr.detectChanges();
  }

  onSubmit(form: NgForm) {
    if (form.invalid) {
      Object.keys(form.controls).forEach(key => form.controls[key].markAsTouched());
      this.cdr.detectChanges();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    const request = this.isLoginMode 
      ? this.apiService.login(this.loginData)
      : this.apiService.signup(this.signupData);

    request.subscribe({
      next: (res) => this.handleAuthSuccess(res),
      error: (err) => this.handleAuthError(err)
    });
  }

  private handleAuthSuccess(response: any) {
    this.isLoading = false;
    this.cdr.detectChanges();
    localStorage.setItem('jwt_token', response.token);
    localStorage.setItem('user_name', response.fullName);
    localStorage.setItem('user_role', response.role);
    const role = (response.role || '').trim();
    const roleHome = role === 'Customer' ? '/customer/products' : '/admin/dashboard';
    this.router.navigate([roleHome]);
  }

  private handleAuthError(error: any) {
    this.isLoading = false; // CRITICAL: Always unfreeze UI
    console.error('Auth Error Payload:', error); // Log for debugging

    if (error.status === 0) {
      this.errorMessage = 'Network Error: Cannot connect to API Gateway. Ensure Ocelot is running on port 5000.';
      this.cdr.detectChanges();
      return;
    }

    // Safely parse .NET Validation Errors (400 Bad Request)
    if (error.error && error.error.errors) {
      this.errorMessage = Object.values(error.error.errors).flat().join(' | ');
      this.cdr.detectChanges();
      return;
    }

    // Parse custom Backend thrown exceptions
    this.errorMessage = error.error?.message || 'Invalid credentials. Please try again.';
    this.cdr.detectChanges();
  }
}
