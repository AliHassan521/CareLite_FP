import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AppointmentService } from '../appointment.service';
import { TokenService } from '../../services/token.service';
import { Appointment } from '../appointment.model';
import { VisitService } from '../../visits/visit.service';
import { Visit } from '../../visits/visit.model';
import { NavbarComponent } from "../../shared/navbar/navbar.component";
import { ProviderCalendarComponent } from '../provider-calendar/provider-calendar.component';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-my-appointments',
  standalone: true,
  imports: [DatePipe, NavbarComponent, ProviderCalendarComponent, RouterModule],
  templateUrl: './my-appointments.component.html',
  styleUrls: ['./my-appointments.component.scss']
})
export class MyAppointmentsComponent implements OnInit {
  appointments: (Appointment & { visit?: Visit | null })[] = [];
  loading = true;
  error: string | null = null;
  private appointmentService = inject(AppointmentService);
  private tokenService = inject(TokenService);
  private visitService = inject(VisitService);
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
        let arr: any[] = [];
        if (Array.isArray(appts)) {
          arr = appts;
        } else if (appts && Array.isArray((appts as any).data)) {
          arr = (appts as any).data;
        } else if (appts && Array.isArray((appts as any).Data)) {
          arr = (appts as any).Data;
        }
        // Fetch visits for each appointment
        Promise.all(arr.map(async a => {
          const appointmentId = a.appointmentId || a.AppointmentId;
          let visit: Visit | null = null;
          try {
            const result = await this.visitService.getVisitByAppointment(appointmentId).toPromise();
            visit = result ?? null;
          } catch (e) {
            visit = null;
          }
          return {
            ...a,
            appointmentId,
            patientName: a.patientName || a.PatientName,
            startTime: a.startTime || a.StartTime,
            durationMinutes: a.durationMinutes || a.DurationMinutes,
            status: a.status || a.Status,
            showHistory: false,
            visit
          };
        })).then(results => {
          this.appointments = results;
          this.loading = false;
        });
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

  markAsCompleted(appt: Appointment) {
    if (this.loading) return;
    this.loading = true;
    const token = this.tokenService.getToken();
    const headers: { [header: string]: string } = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    this.appointmentService.updateAppointment({
      appointmentId: appt.appointmentId,
      patientId: (appt as any).patientId || 0,
      providerId: this.providerId!,
      startTime: appt.startTime,
      newStatus: 'Completed',
      durationMinutes: appt.durationMinutes
    }, headers).subscribe({
      next: (res) => {
        appt.status = 'Completed';
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.Message || 'Failed to update appointment.';
        this.loading = false;
      }
    });
  }
}
