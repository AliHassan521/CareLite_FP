import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { environment } from '../../environments/environment';
import { Router } from '@angular/router';

interface Patient {
  patientId?: number;
  fullName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  gender: string;
  address: string;
  createdAt?: string;
}

@Component({
  selector: 'app-patients',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './patients.component.html',
  styleUrls: ['./patients.component.scss']
})
export class PatientsComponent {
  private http = inject(HttpClient);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  patients: Patient[] = [];
  loading = false;
  error: string | null = null;
  totalCount = 0;

  searchQuery = '';
  page = 1;
  pageSize = 5;

  role: string | null = null;

  constructor() {
    // Get role from token
    const token = localStorage.getItem('token');
    if (!token) {
      this.router.navigate(['/signin']);
      return;
    }
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.role = payload['role'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
      if (!this.role) {
        this.router.navigate(['/signin']);
        return;
      }
    } catch {
      this.role = null;
      this.router.navigate(['/signin']);
      return;
    }

    this.fetchPatients();
  }

  fetchPatients() {
    this.loading = true;
    this.error = null;

    const params = {
      Query: this.searchQuery,
      Page: this.page,
      PageSize: this.pageSize
    };

    this.http.post<{ patients: Patient[], totalCount: number }>(
      `${environment.apiBaseUrl}/patients/search`,
      params
    ).subscribe({
      next: (res) => {
        this.patients = res.patients;
        this.totalCount = res.totalCount;
      },
      error: (err) => this.error = err.error?.Message || 'Failed to fetch patients.'
    }).add(() => this.loading = false);
  }

  onSearch() {
    this.page = 1;
    this.fetchPatients();
  }

  changePage(newPage: number) {
    this.page = newPage;
    this.fetchPatients();
  }

  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/signin']);
  }
}
