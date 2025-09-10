import { Component, signal, effect } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AuditLogsService } from '../services/audit-logs.service';

interface AuditLog {
  action?: string;
  Action?: string;
  user?: string;
  User?: string;
  date?: string;
  Date?: string;
  details?: string;
  Details?: string;
}

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  templateUrl: './audit-logs.component.html',
  styleUrls: ['./audit-logs.component.scss'],
  imports: [DatePipe]
})
export class AuditLogsComponent {
  logs = signal<AuditLog[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  role: string | null = null;

  constructor(private auditLogsService: AuditLogsService) {
    // Decode role from JWT or localStorage (for demo, using localStorage)
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.role = payload['role'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      } catch {
        this.role = null;
      }
    }
    if (this.role === 'Admin') {
      this.fetchLogs();
    }
  }

  fetchLogs() {
    this.loading.set(true);
    this.error.set(null);
    this.auditLogsService.getAll().subscribe({
      next: (res) => {
        if (Array.isArray(res)) {
          this.logs.set(res);
        } else if (res && (res.logs || res.auditLogs)) {
          this.logs.set(res.logs || res.auditLogs);
        } else {
          this.logs.set([]);
        }
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load audit logs.');
        this.loading.set(false);
      }
    });
  }
}
