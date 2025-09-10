import { Component } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from "@angular/common/http";
import { Router, RouterLink } from "@angular/router";
import { environment } from '../../../environments/environment';

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
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router
  ) {
    this.signinForm = this.fb.group({
      username: ["", [Validators.required]],
      password: ["", [Validators.required, Validators.minLength(6)]],
    });
  }

  onSubmit() {
    this.error = null;
    if (this.signinForm.invalid) {
      this.error = "Please enter your username and password.";
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
          } else {
            this.error = "Invalid server response. Token not found.";
          }
        },
        error: (err) => {
          const correlation =
            err.headers?.get("X-Correlation-Id") || err.error?.CorrelationId;
          this.error =
            (err.error?.Message || "Login failed.") +
            (correlation ? ` (Ref: ${correlation})` : "");
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
