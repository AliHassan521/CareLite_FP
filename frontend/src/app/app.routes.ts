import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';
import { RoleGuard } from './guards/role.guard';
import { SigninComponent } from './auth/signin/signin.component';
import { RegisterComponent } from './auth/register/register.component';

export const routes: Routes = [
  { path: '', redirectTo: 'signin', pathMatch: 'full' },
  { path: 'signin', component: SigninComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'dashboard',
    loadComponent: () => import('./dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin', 'Staff', 'Clinician'] }
  },
  {
    path: 'patients',
    loadComponent: () => import('./patients/patients.component').then(m => m.PatientsComponent),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin', 'Staff'] }
  },
  {
    path: 'schedule-appointment',
    loadComponent: () => import('./appointments/schedule-appointment/schedule-appointment.component').then(m => m.ScheduleAppointmentComponent),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin', 'Staff'] }
  },
  {
    path: 'my-appointments',
    loadComponent: () => import('./appointments/my-appointments/my-appointments.component').then(m => m.MyAppointmentsComponent),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Clinician'] }
  },
  {
    path: 'provider-calendar',
    loadComponent: () => import('./appointments/provider-calendar/provider-calendar.component').then(m => m.ProviderCalendarComponent),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin', 'Staff', 'Clinician'] }
  },
  {
    path: 'appointment-status-history/:appointmentId',
    loadComponent: () => import('./appointments/appointment-status/appointment-status-history.component').then(m => m.AppointmentStatusHistoryComponent),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin', 'Staff', 'Clinician'] }
  },
  {
    path: 'audit-log',
    loadComponent: () => import('./audit-log/audit-log.component').then(m => m.AuditLogComponent),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },
  // Add more routes for billing, reports, etc. with appropriate roles as needed
  { path: '**', redirectTo: 'signin' }
];
