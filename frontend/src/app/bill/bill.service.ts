import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environment/environment';
import { Bill } from './bill.model';
import { Observable } from 'rxjs';
import { inject } from '@angular/core';
import { TokenService } from '../services/token.service';

@Injectable({ providedIn: 'root' })
export class BillService {
  private http = inject(HttpClient);
  private tokenService = inject(TokenService);

  getBillByVisit(visitId: number): Observable<Bill | null> {
    const token = this.tokenService.getToken();
    const headers: { [header: string]: string } = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    return this.http.get<{ Data: Bill }>(`${environment.apiBaseUrl}/bill/by-visit/${visitId}`, { headers })
      .pipe(
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
}
