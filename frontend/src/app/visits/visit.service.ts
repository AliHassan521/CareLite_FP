
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environment/environment';
import { Visit } from './visit.model';
import { Observable } from 'rxjs';
import { inject } from '@angular/core';
import { TokenService } from '../services/token.service';

@Injectable({ providedIn: 'root' })

export class VisitService {
  private http = inject(HttpClient);
  private tokenService = inject(TokenService);

  getVisitByAppointment(appointmentId: number): Observable<Visit | null> {
    const token = this.tokenService.getToken();
    const headers: { [header: string]: string } = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    return this.http.get<{ Data: Visit }>(`${environment.apiBaseUrl}/visit/by-appointment/${appointmentId}`, { headers })
      .pipe(
        // If not found, return null
        (source) => new Observable(observer => {
          source.subscribe({
            next: res => observer.next(res.Data),
            error: err => {
              if (err.status === 404) observer.next(null);
              else observer.error(err);
            },
            complete: () => observer.complete()
          });
        })
      );
  }

  createVisit(visit: Visit) {
    const token = this.tokenService.getToken();
    const headers: { [header: string]: string } = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    return this.http.post<{ Data: Visit }>(`${environment.apiBaseUrl}/visit`, visit, { headers });
  }

  updateVisit(visit: Visit) {
    const token = this.tokenService.getToken();
    const headers: { [header: string]: string } = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    return this.http.put<{ Data: Visit }>(`${environment.apiBaseUrl}/visit`, visit, { headers });
  }
}
