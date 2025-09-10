import { Component } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `
  <router-outlet />
  `,
  styles: [],
})
export class AppComponent {
  get isSignedIn() {
    return !!localStorage.getItem('token');
  }
  constructor(private router: Router) {}
  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/signin']);
  }
}
