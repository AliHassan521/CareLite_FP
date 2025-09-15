export interface UpdateAppointmentRequest {
  appointmentId: number;
  patientId: number;
  providerId: number;
  startTime: string;
  newStatus: string;
  durationMinutes: number;
}
