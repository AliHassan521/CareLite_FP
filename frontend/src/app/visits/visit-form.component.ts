import { Component, Input, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { VisitService } from './visit.service';
import { Visit } from './visit.model';
import { ActivatedRoute, Router } from '@angular/router';
import { NavbarComponent } from '../shared/navbar/navbar.component';

@Component({
  selector: 'app-visit-form',
  standalone: true,
  imports: [ReactiveFormsModule, NavbarComponent],
  templateUrl: './visit-form.component.html',
  styleUrls: ['./visit-form.component.scss']
})
export class VisitFormComponent implements OnInit {
  @Input() appointmentId?: number;
  visit: Visit | null = null;
  loading = true;
  error: string | null = null;
  form = inject(FormBuilder).nonNullable.group({
    notes: ['', [Validators.required, Validators.maxLength(2000)]],
  isFinalized: [false as boolean]
  });
  private visitService = inject(VisitService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  ngOnInit() {
    console.log('[VisitForm] ngOnInit, initial appointmentId:', this.appointmentId);
    if (!this.appointmentId) {
      this.route.paramMap.subscribe(params => {
        const paramId = params.get('appointmentId');
        console.log('[VisitForm] route param appointmentId:', paramId);
        this.appointmentId = paramId ? Number(paramId) : undefined;
        this.loadVisit();
      });
    } else {
      this.loadVisit();
    }
  }

  loadVisit() {
    if (!this.appointmentId) return;
    this.loading = true;
    console.log('[VisitForm] Calling getVisitByAppointment with appointmentId:', this.appointmentId);
    this.visitService.getVisitByAppointment(this.appointmentId).subscribe({
      next: visit => {
        console.log('[VisitForm] getVisitByAppointment response:', visit);
        this.visit = visit;
        if (visit) {
          this.form.patchValue({ notes: visit.notes, isFinalized: !!visit.isFinalized });
          if (visit.isFinalized) this.form.disable();
        } else {
          this.form.patchValue({ isFinalized: false });
          this.form.enable();
        }
        this.loading = false;
      },
      error: err => {
        console.error('[VisitForm] getVisitByAppointment error:', err);
        this.error = err.error?.Message || 'Failed to load visit.';
        this.loading = false;
      }
    });
  }

  onSubmit() {
    if (!this.appointmentId || this.form.invalid) return;
    this.loading = true;
    const payload: Visit = {
      appointmentId: this.appointmentId,
      clinicianId: 0,
      notes: this.form.value.notes!,
      isFinalized: !!this.form.value.isFinalized
    };
    if (this.visit) {
      payload.visitId = this.visit.visitId;
      this.visitService.updateVisit({ ...payload, clinicianId: this.visit.clinicianId }).subscribe({
        next: res => {
          console.log('Visit update response:', res);
          if (res && res.Data) {
            this.visit = res.Data;
            this.form.patchValue({ notes: res.Data.notes, isFinalized: res.Data.isFinalized });
            if (res.Data.isFinalized) this.form.disable();
          }
          this.loading = false;
        },
        error: err => {
          this.error = err.error?.Message || 'Failed to update visit.';
          this.loading = false;
        }
      });
    } else {
      this.visitService.createVisit(payload).subscribe({
        next: res => {
          console.log('Visit create response:', res);
          if (res && res.Data) {
            this.visit = res.Data;
            this.form.patchValue({ notes: res.Data.notes, isFinalized: res.Data.isFinalized });
            if (res.Data.isFinalized) this.form.disable();
          }
          this.loading = false;
        },
        error: err => {
          this.error = err.error?.Message || 'Failed to create visit.';
          this.loading = false;
        }
      });
    }
  }
}
