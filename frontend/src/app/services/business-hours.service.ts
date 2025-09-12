import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environment/environment';
import { map, shareReplay } from 'rxjs/operators';
import { Observable } from 'rxjs';

export interface BusinessHours {
  ClinicStart: string;
  ClinicEnd: string;
  BreakStart: string;
  BreakEnd: string;
}

@Injectable({ providedIn: 'root' })
export class BusinessHoursService {
  private businessHours$?: Observable<BusinessHours>;

  constructor(private http: HttpClient) {}

  getBusinessHours(): Observable<BusinessHours> {
    if (!this.businessHours$) {
      this.businessHours$ = this.http.get<BusinessHours>(`${environment.apiBaseUrl}/settings/business-hours`).pipe(
        shareReplay(1)
      );
    }
    return this.businessHours$;
  }
}
