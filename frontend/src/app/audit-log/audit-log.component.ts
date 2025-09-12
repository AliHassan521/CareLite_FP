import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environment/environment';

import { DatePipe } from '@angular/common';
import { NavbarComponent } from "../shared/navbar/navbar.component";
@Component({
  selector: 'app-audit-log',
  standalone: true,
  templateUrl: './audit-log.component.html',
  styleUrls: ['./audit-log.component.scss'],
  imports: [DatePipe, NavbarComponent]
})
export class AuditLogComponent implements OnInit {
  logs: any[] = [];
  loading = true;
  error: string | null = null;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.http.get<any>(`${environment.apiBaseUrl}/auditlog`).subscribe({
      next: res => {
        this.logs = res.data || [];
        this.loading = false;
      },
      error: err => {
        this.error = err.error?.Message || 'Failed to load audit logs.';
        this.loading = false;
      }
    });
  }
}
