import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Appointment } from '../appointment.model';
import { AppointmentStatusHistory } from './appointment-status-history.model';
import { AppointmentStatusHistoryService } from '../../services/appointment-status-history.service';

@Component({
  selector: 'app-appointment-status-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './appointment-status-history.component.html',
  styleUrls: ['./appointment-status-history.component.scss']
})
export class AppointmentStatusHistoryComponent implements OnInit {
  @Input() appointmentId!: number;
  history: AppointmentStatusHistory[] = [];
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
      next: (data) => {
        this.history = data;
        this.loading = false;
      },
      error: (err) => {
        let message = 'Failed to load status history.';
        if (err.error?.Message) {
          message += ' Reason: ' + err.error.Message;
        } else if (err.error) {
          message += ' Response: ' + JSON.stringify(err.error);
        } else if (err.message) {
          message += ' Error: ' + err.message;
        }
        this.error = message;
        this.loading = false;
      }
    });
  }
}
