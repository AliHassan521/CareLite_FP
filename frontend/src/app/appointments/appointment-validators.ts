import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class AppointmentValidators {
  static withinBusinessHours(clinicStart: string, clinicEnd: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      const [hour, minute] = control.value.split(':').map(Number);
      const totalMinutes = hour * 60 + minute;
      const [startHour, startMinute] = clinicStart.split(':').map(Number);
      const [endHour, endMinute] = clinicEnd.split(':').map(Number);
      const start = startHour * 60 + startMinute;
      const end = endHour * 60 + endMinute;
      return totalMinutes < start || totalMinutes >= end
        ? { outsideBusinessHours: true }
        : null;
    };
  }

  static notInBreakTime(breakStart: string, breakEnd: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      const [hour, minute] = control.value.split(':').map(Number);
      const totalMinutes = hour * 60 + minute;
      const [breakStartHour, breakStartMinute] = breakStart.split(':').map(Number);
      const [breakEndHour, breakEndMinute] = breakEnd.split(':').map(Number);
      const breakStartMin = breakStartHour * 60 + breakStartMinute;
      const breakEndMin = breakEndHour * 60 + breakEndMinute;
      return totalMinutes >= breakStartMin && totalMinutes < breakEndMin
        ? { inBreakTime: true }
        : null;
    };
  }

  static notPastDate(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      const selected = new Date(control.value);

      return selected < today ? { pastDate: true } : null;
    };
  }
}
