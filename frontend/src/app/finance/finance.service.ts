import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environment/environment';
import { Observable } from 'rxjs';
import { inject } from '@angular/core';
import { TokenService } from '../services/token.service';

@Injectable({ providedIn: 'root' })
export class FinanceService {
  private http = inject(HttpClient);
  private tokenService = inject(TokenService);

  getOutstandingBalances(patientName?: string): Observable<any[]> {
    const token = this.tokenService.getToken();
    const headers: { [header: string]: string } = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    const params: any = {};
    if (patientName) params.patientName = patientName;
    return this.http.get<{ Data: any[] }>(`${environment.apiBaseUrl}/finance/outstanding-balances`, { headers, params })
      .pipe((source) => new Observable(observer => {
        source.subscribe({
          next: res => observer.next(res.Data),
          error: err => observer.error(err),
          complete: () => observer.complete()
        });
      }));
  }

  getOutstandingBalancesCsv(patientName?: string): Observable<Blob> {
    const token = this.tokenService.getToken();
    const headers: { [header: string]: string } = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    const params: any = { csv: true };
    if (patientName) params.patientName = patientName;
    return this.http.get(`${environment.apiBaseUrl}/finance/outstanding-balances`, { headers, params, responseType: 'blob' });
  }

  getDailyCollections(date: string): Observable<any[]> {
    const token = this.tokenService.getToken();
    const headers: { [header: string]: string } = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    const params: any = { date };
    return this.http.get<{ Data: any[] }>(`${environment.apiBaseUrl}/finance/daily-collections`, { headers, params })
      .pipe((source) => new Observable(observer => {
        source.subscribe({
          next: res => observer.next(res.Data),
          error: err => observer.error(err),
          complete: () => observer.complete()
        });
      }));
  }

  getDailyCollectionsCsv(date: string): Observable<Blob> {
    const token = this.tokenService.getToken();
    const headers: { [header: string]: string } = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    const params: any = { date, csv: true };
    return this.http.get(`${environment.apiBaseUrl}/finance/daily-collections`, { headers, params, responseType: 'blob' });
  }
}
