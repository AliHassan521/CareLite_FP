import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppointmentStatusHistoryModal } from './appointment-status-history.model';
import { AppointmentStatusHistoryService } from '../appointment-status-history.service';

@Component({
  selector: 'app-appointment-status-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './appointment-status-history.component.html',
  styleUrls: ['./appointment-status-history.component.scss']
})
export class AppointmentStatusHistoryComponent implements OnInit {
  @Input() appointmentId!: number;
  history: AppointmentStatusHistoryModal[] = [];
  loading = true;
  error: string | null = null;

  private historyService = inject(AppointmentStatusHistoryService);

  ngOnInit() {
    if (!this.appointmentId) {
      this.error = 'No appointment selected.';
      this.loading = false;
      return;
    }

    this.historyService.getStatusHistory(this.appointmentId).subscribe({
      next: data => {
        this.history = data;
        this.loading = false;
      },
      error: err => {
        this.error = err.error?.Message || 'Failed to load status history.';
        this.loading = false;
      }
    });
  }
}
