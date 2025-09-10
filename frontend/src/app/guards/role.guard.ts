import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {
  constructor(private router: Router) {}

  canActivate(route: any): boolean | UrlTree {
    const token = localStorage.getItem('token');
    if (!token) {
      return this.router.createUrlTree(['/signin']);
    }
    // Decode JWT to get role (simple base64 decode, not secure for production)
    const payload = token.split('.')[1];
    try {
      const decoded = JSON.parse(atob(payload));
      const userRole = decoded['role'] || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      const allowedRoles = route.data?.roles as string[];
      if (allowedRoles && allowedRoles.includes(userRole)) {
        return true;
      }
    } catch {
      // Invalid token
    }
    return this.router.createUrlTree(['/signin']);
  }
}
