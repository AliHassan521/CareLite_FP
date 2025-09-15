import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { MessageService } from '../../services/message.service';
import { environment } from '../../../environment/environment';
import { TokenService } from '../../services/token.service';
import { NavbarComponent } from '../../shared/navbar/navbar.component';
import { AppointmentValidators } from '../appointment-validators';
import { BusinessHoursService, BusinessHours } from '../../services/business-hours.service';
import { take } from 'rxjs/operators';

@Component({
  selector: 'app-schedule-appointment',
  standalone: true,
  imports: [ ReactiveFormsModule, NavbarComponent],
  templateUrl: './schedule-appointment.component.html',
  styleUrls: ['./schedule-appointment.component.scss']
})
export class ScheduleAppointmentComponent implements OnInit {
  form = inject(FormBuilder).nonNullable.group({
    patientId: [null, Validators.required],
    providerId: [null, Validators.required],
    date: ['', [Validators.required, AppointmentValidators.notPastDate()]],
    time: [
      '',
      [Validators.required]
    ],
    durationMinutes: [15, Validators.required]
  });

  businessHours: BusinessHours = {
    ClinicStart: '09:00',
    ClinicEnd: '17:00',
    BreakStart: '13:00',
    BreakEnd: '14:00'
  };

  patients: any[] = [];
  providers: any[] = [];
  loading = false;
  private http = inject(HttpClient);
  private messageService = inject(MessageService);
  private router = inject(Router);
  private tokenService = inject(TokenService);
  private businessHoursService = inject(BusinessHoursService);
  headers: { [header: string]: string } = {};

  constructor() {
    const token = this.tokenService.getToken();
    if (token) this.headers['Authorization'] = `Bearer ${token}`;
  }

  ngOnInit() {
    // Fetch business hours and update validators
    this.businessHoursService.getBusinessHours().pipe(take(1)).subscribe(hours => {
      this.businessHours = hours;
      // Update validators for time control
      this.form.controls.time.setValidators([
        Validators.required,
        AppointmentValidators.withinBusinessHours(hours.ClinicStart, hours.ClinicEnd),
        AppointmentValidators.notInBreakTime(hours.BreakStart, hours.BreakEnd)
      ]);
      this.form.controls.time.updateValueAndValidity();
    });
    // Fetch patients
    this.http.get<any>(`${environment.apiBaseUrl}/patient/search?query=&page=1&pageSize=1000`, { headers: this.headers })
      .subscribe({
        next: res => {
          this.patients = res.patients || [];
          if (!this.patients.length) {
            this.messageService.showMessage({ type: 'info', text: 'No patients found.' }, 4000);
          }
        },
        error: err => {
          this.messageService.showMessage({ type: 'error', text: 'Failed to load patients.' }, 4000);
        }
      });
    // Fetch providers (users with role Clinician)
    this.http.get<any>(`${environment.apiBaseUrl}/users?role=Clinician`, { headers: this.headers })
      .subscribe({
        
        next: res => {
          this.providers = res.data || [];
          //console.log('Providers loaded:', this.providers);
        },
        error: err => {
          this.messageService.showMessage({ type: 'error', text: 'Failed to load providers.' }, 4000);
        }
      });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    const { patientId, providerId, date, time, durationMinutes } = this.form.value;
    // Send local time string to backend (no UTC conversion)
    const startTime = date + 'T' + time; // e.g., '2025-09-12T13:00'
    this.http.post(`${environment.apiBaseUrl}/appointment`, {
      PatientId : Number(patientId),
      ProviderId : Number(providerId),
      StartTime: startTime,
      DurationMinutes : Number(durationMinutes)
    }, { headers: this.headers }).subscribe({
      next: () => {
        this.messageService.showMessage({ type: 'success', text: 'Appointment scheduled!' }, 4000);
        // Redirect to provider calendar and pass providerId for auto-refresh
        this.router.navigate(['/provider-calendar'], { queryParams: { providerId: providerId } });
      },
      error: err => {
        this.messageService.showMessage({ type: 'error', text: err.error?.Message || 'Failed to schedule appointment.' }, 4000);
      }
    }).add(() => this.loading = false);
  }
}

