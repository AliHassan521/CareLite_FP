export interface Visit {
  visitId?: number;
  appointmentId: number;
  clinicianId: number;
  notes: string;
  createdAt?: string;
  updatedAt?: string;
  isFinalized: boolean;
}
