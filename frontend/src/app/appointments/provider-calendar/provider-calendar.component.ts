import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FullCalendarModule } from '@fullcalendar/angular';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environment/environment';
import { TokenService } from '../../services/token.service';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import { NavbarComponent } from "../../shared/navbar/navbar.component";
import { AppointmentService } from '../appointment.service';
import { Appointment } from '../appointment.model';

@Component({
  selector: 'app-provider-calendar',
  standalone: true,
  imports: [CommonModule, FormsModule, FullCalendarModule, NavbarComponent],
  templateUrl: './provider-calendar.component.html',
  styleUrls: ['./provider-calendar.component.scss']
})
export class ProviderCalendarComponent implements OnInit {
  providers: any[] = [];
  selectedProviderId: number | null = null;
  appointments: any[] = [];
  loadingProviders = false;
  loadingAppointments = false;
  weekStart: Date;
  weekEnd: Date;
  weekLabel: string = '';
  private http = inject(HttpClient);
  private tokenService = inject(TokenService);
  private appointmentService = inject(AppointmentService);
  private route = inject(ActivatedRoute);
  headers: { [header: string]: string } = {};

  calendarOptions: any = {
    initialView: 'timeGridWeek',
    headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: 'timeGridDay,timeGridWeek,dayGridMonth'
    },
    slotMinTime: '09:00:00',
    slotMaxTime: '17:00:00',
    allDaySlot: false,
    plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
    events: []
  };

  constructor() {
    // Set current week (Monday to Sunday)
    const now = new Date();
    const day = now.getDay();
    const diffToMonday = (day === 0 ? -6 : 1) - day;
    this.weekStart = new Date(now);
    this.weekStart.setDate(now.getDate() + diffToMonday);
    this.weekStart.setHours(0, 0, 0, 0);
    this.weekEnd = new Date(this.weekStart);
    this.weekEnd.setDate(this.weekStart.getDate() + 6);
    this.weekEnd.setHours(23, 59, 59, 999);
    this.updateWeekLabel();
  }

  ngOnInit() {
    const token = this.tokenService.getToken();
    if (token) this.headers['Authorization'] = `Bearer ${token}`;
    this.loadingProviders = true;
    // Check for providerId in query params
    let initialProviderId: number | null = null;
    this.route.queryParams.subscribe(params => {
      if (params['providerId']) {
        initialProviderId = Number(params['providerId']);
      }
    });
    // Fetch providers (users with role Clinician)
    this.http.get<any>(`${environment.apiBaseUrl}/users?role=Clinician`, { headers: this.headers })
      .subscribe({
        next: res => {
          // Support both Data and data casing
          this.providers = res.data || res.Data || [];
          if (this.providers.length > 0) {
            if (initialProviderId && this.providers.some(p => p.userId === initialProviderId)) {
              this.selectedProviderId = initialProviderId;
            } else {
              this.selectedProviderId = this.providers[0].userId;
            }
            this.fetchAppointments();
          }
          this.loadingProviders = false;
        },
        error: err => {
          this.loadingProviders = false;
        }
      });
  }

  fetchAppointments() {
    if (!this.selectedProviderId) return;
    this.loadingAppointments = true;
    const weekStartIso = this.weekStart.toISOString();
    const weekEndIso = this.weekEnd.toISOString();
    this.appointmentService.getProviderAppointments(this.selectedProviderId, weekStartIso, weekEndIso, this.headers)
      .subscribe({
        next: (appts) => {
          let arr: Appointment[] = [];
          if (Array.isArray(appts)) {
            arr = appts;
          } else if (appts && Array.isArray((appts as any).data)) {
            arr = (appts as any).data;
          } else if (appts && Array.isArray((appts as any).Data)) {
            arr = (appts as any).Data;
          }
          // Normalize fields for calendar (cast to any to avoid TS errors)
          this.appointments = arr.map(a => {
            const appt: any = a;
            return {
              ...appt,
              patientName: appt.patientName || appt.PatientName,
              providerName: appt.providerName || appt.ProviderName,
              providerId: appt.providerId || appt.ProviderId,
              startTime: appt.startTime || appt.StartTime,
              durationMinutes: appt.durationMinutes || appt.DurationMinutes,
              status: appt.status || appt.Status
            };
          });
          this.updateCalendarEvents();
          this.loadingAppointments = false;
        },
        error: () => {
          this.appointments = [];
          this.updateCalendarEvents();
          this.loadingAppointments = false;
        }
      });
  }

  updateCalendarEvents() {
    this.calendarOptions = {
      ...this.calendarOptions,
      events: this.appointments.map(appt => ({
        title: appt.patientName + (appt.status ? ` (${appt.status})` : ''),
        start: appt.startTime,
        end: this.addMinutes(appt.startTime, appt.durationMinutes),
        extendedProps: appt
      }))
    };
  }

  goToPreviousWeek() {
    this.weekStart.setDate(this.weekStart.getDate() - 7);
    this.weekEnd.setDate(this.weekEnd.getDate() - 7);
    this.updateWeekLabel();
    this.fetchAppointments();
  }

  goToNextWeek() {
    this.weekStart.setDate(this.weekStart.getDate() + 7);
    this.weekEnd.setDate(this.weekEnd.getDate() + 7);
    this.updateWeekLabel();
    this.fetchAppointments();
  }

  updateWeekLabel() {
    const options: Intl.DateTimeFormatOptions = { month: 'short', day: 'numeric', year: 'numeric' };
    this.weekLabel = `${this.weekStart.toLocaleDateString(undefined, options)} - ${this.weekEnd.toLocaleDateString(undefined, options)}`;
  }

  // Called when provider is changed from dropdown
  onProviderChange(val: number | null) {
    this.selectedProviderId = val;
    this.fetchAppointments();
  }

  private addMinutes(dateStr: string, minutes: number) {
    const date = new Date(dateStr);
    date.setMinutes(date.getMinutes() + minutes);
    return date.toISOString();
  }
}
