import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AppointmentService } from '../appointment.service';
import { Appointment } from '../appointment.model';
import { TokenService } from '../../services/token.service';
import { RouterModule } from '@angular/router';
import { NavbarComponent } from '../../shared/navbar/navbar.component';

@Component({
  selector: 'app-all-appointments',
  standalone: true,
  imports: [RouterModule, DatePipe, NavbarComponent],
  templateUrl: './all-appointments.component.html',
  styleUrls: ['./all-appointments.component.scss']
})
export class AllAppointmentsComponent implements OnInit {
  appointments: Appointment[] = [];
  loading = true;
  error: string | null = null;
  userRole: string | null = null;
  private appointmentService = inject(AppointmentService);
  private tokenService = inject(TokenService);

  ngOnInit() {
    this.userRole = this.tokenService.getPayload()?.role || null;
    this.loading = true;
    const token = this.tokenService.getToken();
    const headers: { [header: string]: string } = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    this.appointmentService.getAllAppointments(headers).subscribe({
      next: (appts: any) => {
        let arr: any[] = [];
        if (Array.isArray(appts)) {
          arr = appts;
        } else if (appts && Array.isArray(appts.data)) {
          arr = appts.data;
        } else if (appts && Array.isArray(appts.Data)) {
          arr = appts.Data;
        }
        this.appointments = arr.map(a => ({
          ...a,
          patientName: a.patientName || a.PatientName,
          providerName: a.providerName || a.ProviderName,
          providerId: a.providerId || a.ProviderId,
          startTime: a.startTime || a.StartTime,
          durationMinutes: a.durationMinutes || a.DurationMinutes,
          status: a.status || a.Status
        }));
        this.loading = false;
      },
      error: (err: any) => {
        this.error = err.error?.Message || 'Failed to load appointments.';
        this.loading = false;
      }
    });
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
  providerId: (appt as any).providerId || (appt as any).ProviderId || 0,
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
