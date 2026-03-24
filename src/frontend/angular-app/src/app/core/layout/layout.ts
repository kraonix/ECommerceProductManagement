import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, RouterModule],
  templateUrl: './layout.html',
  styleUrl: './layout.scss'
})
export class Layout {
  userName = localStorage.getItem('user_name') || 'Admin';
  userRole = localStorage.getItem('user_role') || 'System';
  normalizedRole = this.userRole.toLowerCase();
  isAdmin = this.normalizedRole === 'admin';
  private router = inject(Router);

  logout() {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('user_name');
    localStorage.removeItem('user_role');
    this.router.navigate(['/auth/login']);
  }
}
