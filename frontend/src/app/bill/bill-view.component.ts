import { Component, Input, OnInit, inject } from '@angular/core';
import { BillService } from './bill.service';
import { Bill } from './bill.model';
import { DecimalPipe } from '@angular/common';
@Component({
  selector: 'app-bill-view',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './bill-view.component.html',
  styleUrls: ['./bill-view.component.scss']
})
export class BillViewComponent implements OnInit {
  @Input() visitId!: number;
  bill: Bill | null = null;
  loading = true;
  error: string | null = null;
  private billService = inject(BillService);

  ngOnInit() {
    if (!this.visitId) return;
    this.loading = true;
    this.billService.getBillByVisit(this.visitId).subscribe({
      next: (bill: Bill | null) => {
        this.bill = bill;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = err.error?.Message || 'Failed to load bill.';
        this.loading = false;
      }
    });
  }
}
