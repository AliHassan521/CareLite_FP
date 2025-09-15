import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environment/environment';
import { UpdateAppointmentRequest } from './all-appointments/update-appointment-request.model';

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  constructor(private http: HttpClient) {}

  getProviderAppointments(providerId: number, weekStart: string, weekEnd: string, headers?: { [header: string]: string }) {
    return this.http.get<any[]>(
      `${environment.apiBaseUrl}/appointment/provider/${providerId}?weekStart=${weekStart}&weekEnd=${weekEnd}`,
      headers ? { headers } : {}
    );
  }

  getAllAppointments(headers?: { [header: string]: string }) {
    return this.http.get<any[]>(
      `${environment.apiBaseUrl}/appointment/all`,
      headers ? { headers } : {}
    );
  }

  updateAppointment(request: UpdateAppointmentRequest, headers?: { [header: string]: string }) {
    return this.http.put<any>(
      `${environment.apiBaseUrl}/appointment`,
      request,
      headers ? { headers } : {}
    );
  }
}
