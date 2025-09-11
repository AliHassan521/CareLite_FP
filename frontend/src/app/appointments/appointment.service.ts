import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environment/environment';

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  constructor(private http: HttpClient) {}

  getProviderAppointments(providerId: number, weekStart: string, weekEnd: string, headers?: { [header: string]: string }) {
    return this.http.get<any[]>(
      `${environment.apiBaseUrl}/appointment/provider/${providerId}?weekStart=${weekStart}&weekEnd=${weekEnd}`,
      headers ? { headers } : {}
    );
  }
}
