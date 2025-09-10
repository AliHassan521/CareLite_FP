import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TokenService {
  getPayload() {
    const token = localStorage.getItem('token');
    if (!token) return null;
    try {
      return JSON.parse(atob(token.split('.')[1]));
    } catch {
      return null;
    }
  }

  getRole() {
    const payload = this.getPayload();
    return payload?.role || payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
  }

  isExpired() {
    const payload = this.getPayload();
    const now = Math.floor(Date.now() / 1000);
    return payload?.exp && payload.exp < now;
  }

  getToken() {
    return localStorage.getItem('token');
  }
}
