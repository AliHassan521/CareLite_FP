import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environment/environment';
import { Payment } from './payment.model';
import { Observable } from 'rxjs';
import { inject } from '@angular/core';
import { TokenService } from '../services/token.service';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private http = inject(HttpClient);
  private tokenService = inject(TokenService);

  recordPayment(billId: number, amount: number, method: 'Cash' | 'Card'): Observable<{ TotalAmount: number; RemainingBalance: number }> {
    const token = this.tokenService.getToken();
    const headers: { [header: string]: string } = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    return this.http.post<{ TotalAmount: number; RemainingBalance: number }>(`${environment.apiBaseUrl}/payment/record?billId=${billId}&amount=${amount}&method=${method}`, {}, { headers });
  }
}
