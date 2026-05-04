import { Component, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';

export enum AuthMode {
  LOGIN,
  SIGNUP,
  FORGOT_PASSWORD,
  RESET_PASSWORD
}

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  // Enum exposure for HTML template binding
  AuthMode = AuthMode;
  currentMode: AuthMode = AuthMode.LOGIN;

  isLoading = false;
  showPassword = false;
  errorMessage = '';
  successMessage = '';

  // Data bindings
  loginData = { email: '', password: '' };
  signupData = { fullName: '', email: '', password: '', role: 'Admin' };
  forgotData = { email: '' };
  resetData = { token: '', newPassword: '' };

  private apiService = inject(ApiService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  setMode(mode: AuthMode, form?: NgForm) {
    this.currentMode = mode;
    this.errorMessage = '';
    this.successMessage = '';
    if (form) form.resetForm();
    // Reset role after form reset so Angular's form control re-initialization
    // doesn't wipe the value we set
    if (mode === AuthMode.SIGNUP) {
      setTimeout(() => {
        this.signupData.role = 'Admin';
        this.cdr.detectChanges();
      });
    }
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
    this.successMessage = '';
    this.cdr.detectChanges();

    if (this.currentMode === AuthMode.LOGIN) {
      this.apiService.login(this.loginData).subscribe({
        next: (res) => this.handleAuthSuccess(res),
        error: (err) => this.handleAuthError(err)
      });
    } else if (this.currentMode === AuthMode.SIGNUP) {
      this.apiService.signup(this.signupData).subscribe({
        next: (res) => this.handleAuthSuccess(res),
        error: (err) => this.handleAuthError(err)
      });
    } else if (this.currentMode === AuthMode.FORGOT_PASSWORD) {
      this.apiService.forgotPassword(this.forgotData.email).subscribe({
        next: (res) => {
          this.isLoading = false;
          // Capture the generated token explicitly returned from backend mock logic
          const simulatedToken = res.resetToken; 
          this.successMessage = `Instructions prepared! (Auto-redirecting to Reset panel simply to mimic email click...)`;
          this.cdr.detectChanges();
          
          setTimeout(() => {
            this.resetData.token = simulatedToken; // Auto-populate the mock token
            this.setMode(AuthMode.RESET_PASSWORD);
            this.successMessage = 'Paste the temporary token received in your email.';
            this.cdr.detectChanges();
          }, 2500);
        },
        error: (err) => this.handleAuthError(err)
      });
    } else if (this.currentMode === AuthMode.RESET_PASSWORD) {
      this.apiService.resetPassword(this.resetData.token, this.resetData.newPassword).subscribe({
        next: (res) => {
          this.isLoading = false;
          this.successMessage = 'Password universally updated! You can now log in safely.';
          this.setMode(AuthMode.LOGIN);
          this.cdr.detectChanges();
        },
        error: (err) => this.handleAuthError(err)
      });
    }
  }

  private handleAuthSuccess(response: any) {
    this.isLoading = false;
    this.cdr.detectChanges();
    
    // Explicit extended auth cache insertion 
    localStorage.setItem('jwt_token', response.token);
    localStorage.setItem('refresh_token', response.refreshToken);
    localStorage.setItem('user_name', response.fullName);
    localStorage.setItem('user_role', response.role);
    
    const role = (response.role || '').trim();
    const roleHome = role === 'Customer' ? '/customer/products' : '/admin/dashboard';
    this.router.navigate([roleHome]);
  }

  private handleAuthError(error: any) {
    this.isLoading = false;
    console.error('Core Auth Rejection Payload:', error);

    if (error.status === 0) {
      this.errorMessage = 'Cannot reach the server. Please verify all services are running.';
    } else if (error.status === 401) {
      // Wrong credentials — give a clear, direct message
      if (this.currentMode === AuthMode.LOGIN) {
        this.errorMessage = 'Incorrect email or password. Please try again.';
      } else {
        this.errorMessage = error.error?.detail || error.error?.message || 'Authentication failed.';
      }
    } else if (error.status === 429) {
      this.errorMessage = 'Too many attempts. Please wait a moment before trying again.';
    } else if (error.error && error.error.detail) {
      // Direct Exception Mapping via GlobalExceptionHandler
      this.errorMessage = error.error.detail;
    } else if (error.error && error.error.errors) {
      // Standard validation boundaries breaking
      this.errorMessage = Object.values(error.error.errors).flat().join(' | ');
    } else {
      this.errorMessage = error.error?.message || 'Something went wrong. Please try again.';
    }
    this.cdr.detectChanges();
  }
}
