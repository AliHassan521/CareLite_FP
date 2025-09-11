import { Component } from '@angular/core';
import { MessageService } from '../../services/message.service';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from "@angular/common/http";
import { Router, RouterLink } from "@angular/router";
import { environment } from '../../../environment/environment';

@Component({
  selector: "app-signin",
  standalone: true,
  templateUrl: "./signin.component.html",
  styleUrls: ["./signin.component.scss"],
  imports: [ReactiveFormsModule, RouterLink],
})
export class SigninComponent {
  private baseUrl = environment.apiBaseUrl;
  signinForm: FormGroup;
  loading = false;
  error: string | null = null; // Only for form validation

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
  private router: Router,
  private messageService: MessageService
  ) {
    this.signinForm = this.fb.group({
      username: ["", [Validators.required]],
      password: ["", [Validators.required, Validators.minLength(6)]],
    });
  }

  onSubmit() {
    this.error = null;
    if (this.signinForm.invalid) {
      this.messageService.showMessage({ type: 'error', text: 'Please enter your username and password.' }, 4000);
      return;
    }
    this.loading = true;
    this.http
      .post<any>(
        `${this.baseUrl}/auth/login`,
        this.signinForm.value,
        { observe: "response" }
      )
      .subscribe({
        next: (res) => {
          if (res && res.body && (res.body.Token || res.body.token)) {
            const token = res.body.Token || res.body.token;
            localStorage.setItem("token", token);
            this.router.navigate(["/dashboard"]);
            this.messageService.showMessage({ type: 'success', text: 'Signed in successfully!' }, 4000);
          } else {
            this.messageService.showMessage({ type: 'error', text: 'Invalid server response. Token not found.' }, 4000);
          }
        },
        error: (err) => {
          const correlation =
            err.headers?.get("X-Correlation-Id") || err.error?.CorrelationId;
          this.messageService.showMessage({
            type: 'error',
            text: (err.error?.Message || 'Login failed.') + (correlation ? ` (Ref: ${correlation})` : '')
          }, 4000);
        },
      })
      .add(() => (this.loading = false));
  }

  get sessionExpired(): boolean {
    const params = new URLSearchParams(window.location.search);
    return params.get('session') === 'expired';
  }

  logout() {
    localStorage.removeItem("token");
    this.router.navigate(["/signin"]);
  }
}
