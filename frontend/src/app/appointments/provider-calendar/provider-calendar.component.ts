import { Component, Input, OnInit } from '@angular/core';
import { FullCalendarModule } from '@fullcalendar/angular';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';

@Component({
  selector: 'app-provider-calendar',
  standalone: true,
  imports: [FullCalendarModule],
  templateUrl: './provider-calendar.component.html',
  styleUrls: ['./provider-calendar.component.scss']
})
export class ProviderCalendarComponent implements OnInit {
  @Input() appointments: any[] = [];

  calendarOptions: any = {
    initialView: 'timeGridWeek',
    headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: 'timeGridDay,timeGridWeek,dayGridMonth'
    },
    slotMinTime: '09:00:00',
    slotMaxTime: '17:00:00',
    allDaySlot: false,
    plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
    events: []
  };

  ngOnInit() {
    this.calendarOptions.events = this.appointments.map(a => ({
      id: a.appointmentId,
      title: a.patientName,
      start: a.startTime,
      end: this.addMinutes(a.startTime, a.durationMinutes),
      backgroundColor: a.status === 'Canceled' ? 'red' : '#3788d8'
    }));
  }

  private addMinutes(dateStr: string, minutes: number) {
    const date = new Date(dateStr);
    date.setMinutes(date.getMinutes() + minutes);
    return date.toISOString();
  }
}
