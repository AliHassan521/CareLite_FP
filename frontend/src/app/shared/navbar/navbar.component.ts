import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {
  role: string | null = null;
  constructor(private router: Router) {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.role = payload['role'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
      } catch {
        this.role = null;
      }
    }
  }
  goTo(path: string) {
    this.router.navigate([path]);
  }
  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/signin']);
  }
}
