import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { NavbarComponent } from '../shared/navbar/navbar.component';

import { TokenService } from '../services/token.service';



@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [NavbarComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  role: string | null = null;
  userName: string | null = null;
  fullName: string | null = null;
  private router = inject(Router);
  private tokenService = inject(TokenService);

  constructor() {
    const payload = this.tokenService.getPayload();
    //console.log('Token payload:', payload);
    this.role = payload?.role || payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
    
    this.userName = payload?.unique_name || payload?.username || payload?.email || null;
    this.fullName = payload?.given_name || null;
    if (!this.role) {
      this.router.navigate(['/signin']);
      return;
    }
  }
}
