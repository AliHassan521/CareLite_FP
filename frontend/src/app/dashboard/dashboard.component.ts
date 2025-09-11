import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { NavbarComponent } from '../shared/navbar.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [NavbarComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  role: string | null = null;
  loading = true;

  constructor(private router: Router) {
    const token = localStorage.getItem('token');
    if (!token) {
      this.router.navigate(['/signin']);
      return;
    }
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.role = payload['role'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
      if (!this.role) {
        // If role is missing, treat as invalid session
        this.router.navigate(['/signin']);
        return;
      }
    } catch {
      this.role = null;
      this.router.navigate(['/signin']);
      return;
    }
    this.loading = false;
  }
}
