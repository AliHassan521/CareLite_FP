import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../environment/environment';
import { map, shareReplay } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { TokenService } from './token.service';

export interface BusinessHours {
  ClinicStart: string;
  ClinicEnd: string;
  BreakStart: string;
  BreakEnd: string;
}

@Injectable({ providedIn: 'root' })
export class BusinessHoursService {
  private businessHours$?: Observable<BusinessHours>;

  constructor(private http: HttpClient, private tokenService: TokenService) {}

  getBusinessHours(): Observable<BusinessHours> {
    if (!this.businessHours$) {
      const token = this.tokenService.getToken();
      let headers = {};
      if (token) {
        headers = { Authorization: `Bearer ${token}` };
      }
      this.businessHours$ = this.http.get<BusinessHours>(
        `${environment.apiBaseUrl}/settings/business-hours`,
        { headers }
      ).pipe(shareReplay(1));
    }
    return this.businessHours$;
  }
}
