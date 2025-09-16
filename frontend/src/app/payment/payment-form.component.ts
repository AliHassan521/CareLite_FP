import { Component, Input, OnInit, inject } from '@angular/core';
import { PaymentService } from './payment.service';
import { Bill } from '../bill/bill.model';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-payment-form',
  standalone: true,
  imports: [ReactiveFormsModule, DecimalPipe],
  templateUrl: './payment-form.component.html',
  styleUrls: ['./payment-form.component.scss']
})
export class PaymentFormComponent implements OnInit {
  @Input() bill!: Bill;
  @Input() remainingBalance!: number;
  loading = false;
  error: string | null = null;
  success: string | null = null;
  private paymentService = inject(PaymentService);
  form = inject(FormBuilder).nonNullable.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    method: ['Cash', [Validators.required]]
  });

  ngOnInit() {}

  onSubmit() {
    if (this.form.invalid || !this.bill) return;
    const amount = Number(this.form.value.amount ?? 0);
    if (amount > this.remainingBalance) {
      this.error = 'Payment exceeds remaining balance.';
      return;
    }
    this.loading = true;
  const method: 'Cash' | 'Card' = this.form.value.method === 'Card' ? 'Card' : 'Cash';
  this.paymentService.recordPayment(this.bill.billId, amount, method).subscribe({
      next: (res: { TotalAmount: number; RemainingBalance: number }) => {
        this.success = 'Payment recorded.';
        this.error = null;
        this.loading = false;
        this.form.reset({ amount: 0, method: 'Cash' });
        this.remainingBalance = res.RemainingBalance;
      },
      error: (err: any) => {
        this.error = err.error?.Message || 'Failed to record payment.';
        this.success = null;
        this.loading = false;
      }
    });
  }
}
