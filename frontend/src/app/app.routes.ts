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
    canActivate: [AuthGuard]
  },
  {
    path: 'patients',
    loadComponent: () => import('./patients/patient-list/patients-list.component').then(m => m.PatientListComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'audit-logs',
    loadComponent: () => import('./audit-logs/audit-logs.component').then(m => m.AuditLogsComponent),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },
  { path: '**', redirectTo: 'signin' }
];
