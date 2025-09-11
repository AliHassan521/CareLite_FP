import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink],
  template: `
    <nav class="navbar">
      <div class="brand" (click)="goTo('/dashboard')" style="cursor:pointer; font-weight:600;">CareLite</div>
      <div class="links">
        <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
        <a routerLink="/patients" routerLinkActive="active">Patients</a>
        <a routerLink="/schedule-appointment" routerLinkActive="active">Schedule Appointment</a>
        <!-- Add more links here as needed -->
      </div>
      <div class="user-info">
        <button (click)="logout()">Logout</button>
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      display: flex; justify-content: space-between; align-items: center;
      padding: 0.5rem 1rem; background: #1976d2; color: #fff;
      margin-bottom: 2rem;
    }
    .brand { font-size: 1.3rem; letter-spacing: 0.04em; }
    .links a {
      color: #fff; margin-left: 1rem; cursor: pointer; text-decoration: none; font-weight: 500;
      padding: 0.3rem 0.7rem; border-radius: 5px; transition: background 0.2s, color 0.2s;
    }
    .links a.active, .links a:hover {
      background: #e3eafc; color: #1565c0;
    }
    .user-info { display: flex; align-items: center; }
    .user-info button {
      margin-left: 1rem; cursor: pointer; background: #fff; color: #1976d2; border: none;
      padding: 0.35rem 1.2rem; border-radius: 5px; font-weight: 500; transition: background 0.2s, color 0.2s;
    }
    .user-info button:hover { background: #e3eafc; color: #1565c0; }
  `]
})
export class NavbarComponent {
  constructor(private router: Router) {}
  goTo(path: string) {
    this.router.navigate([path]);
  }
  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/signin']);
  }
}
