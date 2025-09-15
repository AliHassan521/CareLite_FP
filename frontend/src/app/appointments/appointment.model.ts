export interface Appointment {
  appointmentId: number;
  patientName: string;
  startTime: string;
  durationMinutes: number;
  status: string;
  providerName?: string;
  providerId?: number;
  showHistory?: boolean;
}
