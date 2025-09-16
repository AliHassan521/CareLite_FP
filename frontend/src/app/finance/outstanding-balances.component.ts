import { Component, OnInit, inject } from '@angular/core';
import { FinanceService } from './finance.service';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-outstanding-balances',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './outstanding-balances.component.html',
  styleUrls: ['./outstanding-balances.component.scss']
})
export class OutstandingBalancesComponent implements OnInit {
  balances: any[] = [];
  loading = false;
  error: string | null = null;
  private financeService = inject(FinanceService);
  form = inject(FormBuilder).nonNullable.group({
    patientName: ['']
  });

  ngOnInit() {
    this.loadBalances();
  }

  loadBalances() {
    this.loading = true;
    this.financeService.getOutstandingBalances(this.form.value.patientName).subscribe({
      next: (data: any[]) => {
        this.balances = data;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = err.error?.Message || 'Failed to load balances.';
        this.loading = false;
      }
    });
  }

  exportCsv() {
    this.financeService.getOutstandingBalancesCsv(this.form.value.patientName).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'outstanding_balances.csv';
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }
}
