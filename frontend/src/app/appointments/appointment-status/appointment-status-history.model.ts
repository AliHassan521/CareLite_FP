export interface AppointmentStatusHistory {
  historyId: number;
  appointmentId: number;
  oldStatus: string | null;
  newStatus: string;
  changedAt: string; // ISO string
  changedBy: number | null;
}
