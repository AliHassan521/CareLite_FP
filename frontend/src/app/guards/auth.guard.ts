import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private router: Router) {}

  canActivate(): boolean | UrlTree {
    const token = localStorage.getItem('token');
    if (!token) {
      return this.router.createUrlTree(['/signin'], { queryParams: { session: 'expired' } });
    }
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp;
      const now = Math.floor(Date.now() / 1000);
      if (exp && exp < now) {
        localStorage.removeItem('token');
        return this.router.createUrlTree(['/signin'], { queryParams: { session: 'expired' } });
      }
      return true;
    } catch {
      localStorage.removeItem('token');
      return this.router.createUrlTree(['/signin'], { queryParams: { session: 'expired' } });
    }
  }
}
