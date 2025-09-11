import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class AppointmentValidators {
  static withinBusinessHours(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      const [hour, minute] = control.value.split(':').map(Number);
      const totalMinutes = hour * 60 + minute;
      const start = 9 * 60;   // 09:00
      const end = 17 * 60;    // 17:00

      return totalMinutes < start || totalMinutes > end
        ? { outsideBusinessHours: true }
        : null;
    };
  }

  static notInBreakTime(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      const [hour, minute] = control.value.split(':').map(Number);
      const totalMinutes = hour * 60 + minute;
      const breakStart = 13 * 60; // 13:00
      const breakEnd = 14 * 60;   // 14:00

      return totalMinutes >= breakStart && totalMinutes < breakEnd
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
