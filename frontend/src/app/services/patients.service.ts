import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Patient } from '../patients/patient.model';

@Injectable({ providedIn: 'root' })
export class PatientService {
  private baseUrl = environment.apibaseUrl;

  constructor(private http: HttpClient) {}

  getPatients() {
    return this.http.get<Patient[]>(`${this.baseUrl}/patients`);
  }

  getPatientById(id: number) {
    return this.http.get<Patient>(`${this.baseUrl}/patients/${id}`);
  }

  createPatient(patient: Patient) {
    return this.http.post<Patient>(`${this.baseUrl}/patients`, patient);
  }

  updatePatient(id: number, patient: Patient) {
    return this.http.put<Patient>(`${this.baseUrl}/patients/${id}`, patient);
  }

  deletePatient(id: number) {
    return this.http.delete(`${this.baseUrl}/patients/${id}`);
  }
}
