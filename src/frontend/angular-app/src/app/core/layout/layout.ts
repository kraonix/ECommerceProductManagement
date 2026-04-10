import { Component, inject, signal } from '@angular/core';
import { RouterOutlet, RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, RouterModule, CommonModule],
  templateUrl: './layout.html',
  styleUrl: './layout.scss'
})
export class Layout {
  userName = localStorage.getItem('user_name') || 'Admin';
  userRole = localStorage.getItem('user_role') || 'System';
  normalizedRole = this.userRole.toLowerCase();
  isAdmin = this.normalizedRole === 'admin';
  private router = inject(Router);

  // Sidebar collapse state
  sidebarCollapsed = signal(false);
  userMenuOpen = signal(false);

  toggleSidebar() { this.sidebarCollapsed.update(v => !v); }
  toggleUserMenu() { this.userMenuOpen.update(v => !v); }
  closeUserMenu() { this.userMenuOpen.set(false); }

  logout() {
    localStorage.clear();
    this.router.navigate(['/auth/login']);
  }
}
