import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AppointmentService } from '../appointment.service';
import { TokenService } from '../../services/token.service';
import { Appointment } from '../appointment.model';
import { NavbarComponent } from "../../shared/navbar/navbar.component";
import { ProviderCalendarComponent } from '../provider-calendar/provider-calendar.component';
import { AppointmentStatusHistoryComponent } from '../appointment-status/appointment-status-history.component';

@Component({
  selector: 'app-my-appointments',
  standalone: true,
  imports: [DatePipe, NavbarComponent, ProviderCalendarComponent, AppointmentStatusHistoryComponent],
  templateUrl: './my-appointments.component.html',
  styleUrls: ['./my-appointments.component.scss']
})
export class MyAppointmentsComponent implements OnInit {
  appointments: Appointment[] = [];
  loading = true;
  error: string | null = null;
  private appointmentService = inject(AppointmentService);
  private tokenService = inject(TokenService);
  providerId: number | null = null;

  ngOnInit() {
    const payload = this.tokenService.getPayload();
    this.providerId = payload?.userId || payload?.sub || payload?.nameid || null;
    if (!this.providerId) {
      this.error = 'Unable to determine provider ID.';
      this.loading = false;
      return;
    }
    // Get current week range
    const now = new Date();
    const day = now.getDay();
    const diffToMonday = (day === 0 ? -6 : 1) - day;
    const monday = new Date(now);
    monday.setDate(now.getDate() + diffToMonday);
    monday.setHours(0, 0, 0, 0);
    const sunday = new Date(monday);
    sunday.setDate(monday.getDate() + 6);
    sunday.setHours(23, 59, 59, 999);
    const weekStart = monday.toISOString();
    const weekEnd = sunday.toISOString();
    const token = this.tokenService.getToken();
    const headers: { [header: string]: string } = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    this.appointmentService.getProviderAppointments(this.providerId, weekStart, weekEnd, headers).subscribe({
      next: (appts) => {
        this.appointments = appts.map(a => ({ ...a, patientName: a.patientName || a.PatientName }));
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.Message || 'Failed to load appointments.';
        this.loading = false;
      }
    });
  }

  trackById(index: number, appt: Appointment) {
    return appt.appointmentId;
  }
}
