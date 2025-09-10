import { Component } from '@angular/core';
import { MessageService } from '../../services/message.service';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-register',
  standalone: true,
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  imports: [ReactiveFormsModule, RouterLink]
})
export class RegisterComponent {
  private baseUrl = environment.apibaseUrl;
  registerForm: FormGroup;
  loading = false;
  error: string | null = null; // Only for form validation

  roles = ['Admin', 'Staff', 'Clinician'];
  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router, private messageService: MessageService) {
    this.registerForm = this.fb.group({
      username: ['', [Validators.required]],
      fullName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      roleName: ['Staff', [Validators.required]]
    });
  }

  onSubmit() {
    this.error = null;
    if (this.registerForm.invalid) {
      this.messageService.showMessage({ type: 'error', text: 'Please fill all required fields correctly.' }, 4000);
      return;
    }
    this.loading = true;
    this.http.post<any>(`${this.baseUrl}/auth/register`, this.registerForm.value, { observe: 'response' })
      .subscribe({
        next: (res) => {
          this.messageService.showMessage({ type: 'success', text: 'Registration successful! Please sign in.' }, 4000);
          this.registerForm.reset();
          setTimeout(() => this.router.navigate(['/signin']), 1200);
        },
        error: (err) => {
          const correlation = err.headers?.get('X-Correlation-Id') || err.error?.CorrelationId;
          this.messageService.showMessage({
            type: 'error',
            text: (err.error?.Message || 'Registration failed.') + (correlation ? ` (Ref: ${correlation})` : '')
          }, 4000);
        }
      }).add(() => this.loading = false);
  }
}
