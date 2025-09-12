
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { AppointmentStatusHistory } from './appointment-status/appointment-status-history.model';

@Injectable({ providedIn: 'root' })
export class AppointmentStatusHistoryService {
  private readonly apiUrl = '/api/appointment';

  constructor(private http: HttpClient) {}

  getStatusHistory(appointmentId: number): Observable<AppointmentStatusHistory[]> {
    return this.http.get<{ data: AppointmentStatusHistory[] }>(`${this.apiUrl}/${appointmentId}/status-history`)
      .pipe(
        map((res: { data: AppointmentStatusHistory[] }) => res.data)
      );
  }
}
