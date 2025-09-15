import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from './rxjs-helpers';
import { debounceTime, distinctUntilChanged } from './rxjs-helpers';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { environment } from '../../environment/environment';
import { TokenService } from '../services/token.service';
import { Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { MessageService } from '../services/message.service';
import {Patient} from './patient.model'
import { NavbarComponent } from '../shared/navbar/navbar.component';

// interface Patient {
//   patientId?: number;
//   fullName: string;
//   email: string;
//   phone: string;
//   dateOfBirth: string;
//   gender: string;
//   address: string;
//   createdAt?: string;
// }

@Component({
  selector: 'app-patients',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe, NavbarComponent],
  templateUrl: './patients.component.html',
  styleUrls: ['./patients.component.scss']
})
export class PatientsComponent implements OnInit, OnDestroy {
  private searchSub: Subscription | undefined;
  private http = inject(HttpClient);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private tokenService = inject(TokenService);
  private messageService = inject(MessageService);

  patients: Patient[] = [];
  loading = false;
  error: string | null = null; 
  totalCount = 0;

  searchQuery = '';
  searchForm = this.fb.group({
    query: ['']
  });
  page = 1;
  pageSize = 5;

  role: string | null = null;

  addPatientForm = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', Validators.required],
    dateOfBirth: ['', Validators.required],
    gender: ['', Validators.required],
    address: ['', Validators.required]
  });
  addPatientLoading = false;
  duplicateMatches: Patient[] = [];
  showDuplicateWarning = false;
  pendingPatient: Patient | null = null;

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

  ngOnInit() {
    // Debounce search input
    this.searchSub = this.searchForm.get('query')?.valueChanges
      ?.pipe(
        debounceTime(400),
        distinctUntilChanged()
      )
          .subscribe((val: string | null) => {
        this.page = 1;
            this.searchQuery = val ?? '';
        this.fetchPatients();
      });
  }

  ngOnDestroy() {
    if (this.searchSub) this.searchSub.unsubscribe();
  }
    addPatient() {
      if (this.addPatientForm.invalid) return;
      this.duplicateMatches = [];
      this.showDuplicateWarning = false;
      this.pendingPatient = null;

      // Check for duplicates in loaded patients (by email or phone)
      const rawValue = this.addPatientForm.getRawValue();
      const formValue: Patient = {
        fullName: rawValue.fullName || '',
        email: rawValue.email || '',
        phone: rawValue.phone || '',
        dateOfBirth: rawValue.dateOfBirth || '',
        gender: rawValue.gender || '',
        address: rawValue.address || ''
      };
      const matches = this.patients.filter(p =>
        (p.email && formValue.email && p.email.toLowerCase() === formValue.email.toLowerCase()) ||
        (p.phone && formValue.phone && p.phone === formValue.phone)
      );
      if (matches.length > 0) {
        this.duplicateMatches = matches;
        this.showDuplicateWarning = true;
        this.pendingPatient = formValue;
        this.messageService.showMessage({ type: 'warning', text: 'Potential duplicate(s) found. Please review.' }, 4000);
        return;
      }
      this._createPatient(formValue);
    }

    confirmAddPatient() {
      if (!this.pendingPatient) return;
      this.showDuplicateWarning = false;
      this._createPatient(this.pendingPatient);
      this.pendingPatient = null;
    }

    cancelAddPatient() {
      this.showDuplicateWarning = false;
      this.pendingPatient = null;
    }

    private _createPatient(patient: Patient) {
      this.addPatientLoading = true;
      const token = this.tokenService.getToken();
      const headers: { [header: string]: string } = {};
      if (token) headers['Authorization'] = `Bearer ${token}`;
      this.http.post(`${environment.apiBaseUrl}/patient`, patient, { headers })
        .subscribe({
          next: (res) => {
            this.messageService.showMessage({ type: 'success', text: 'Patient added successfully!' }, 4000);
            this.addPatientForm.reset();
            this.fetchPatients();
          },
          error: (err) => {
            this.messageService.showMessage({ type: 'error', text: err.error?.Message || 'Failed to add patient.' }, 4000);
          }
        }).add(() => this.addPatientLoading = false);
    }

  fetchPatients() {
    this.loading = true;
    this.error = null;

    const params = {
      query: this.searchQuery,
      page: this.page,
      pageSize: this.pageSize
    };

    const token = this.tokenService.getToken();
    const headers: { [header: string]: string } = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    this.http.get<{ patients: Patient[]; total: number }>(
      `${environment.apiBaseUrl}/patient/search`,
      { params, headers }
    ).subscribe({
      next: (res) => {
        // Normalize fields and sort by fullName (case-insensitive, handle missing values)
        this.patients = (res.patients || []).map(p => {
          const pat: any = p;
          return {
            ...pat,
            patientId: pat.patientId || pat.PatientId,
            fullName: pat.fullName || pat.FullName,
            email: pat.email || pat.Email,
            phone: pat.phone || pat.Phone,
            dateOfBirth: pat.dateOfBirth || pat.DateOfBirth,
            gender: pat.gender || pat.Gender,
            address: pat.address || pat.Address,
            createdAt: pat.createdAt || pat.CreatedAt
          };
        }).sort((a, b) => {
          const nameA = a.fullName || '';
          const nameB = b.fullName || '';
          return nameA.localeCompare(nameB, undefined, { sensitivity: 'base' });
        });
        this.totalCount = res.total;
      },
      error: (err) => this.error = err.error?.Message || 'Failed to fetch patients.'
    }).add(() => this.loading = false);
  }

  onSearch() {
  this.page = 1;
  this.searchQuery = this.searchForm.value.query || '';
  this.fetchPatients();
  }

  changePage(newPage: number) {
    this.page = newPage;
    this.fetchPatients();
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/signin']);
  }
}
