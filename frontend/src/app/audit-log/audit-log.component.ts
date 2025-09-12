import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environment/environment';

@Component({
  selector: 'app-audit-log',
  standalone: true,
  templateUrl: './audit-log.component.html',
  styleUrls: ['./audit-log.component.scss']
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
