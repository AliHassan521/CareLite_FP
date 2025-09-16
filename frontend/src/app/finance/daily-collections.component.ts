import { Component, OnInit, inject } from '@angular/core';
import { FinanceService } from './finance.service';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-daily-collections',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './daily-collections.component.html',
  styleUrls: ['./daily-collections.component.scss']
})
export class DailyCollectionsComponent implements OnInit {
  collections: any[] = [];
  loading = false;
  error: string | null = null;
  private financeService = inject(FinanceService);
  form = inject(FormBuilder).nonNullable.group({
    date: ['']
  });

  ngOnInit() {
    this.form.patchValue({ date: this.today() });
    this.loadCollections();
  }

  today(): string {
    const d = new Date();
    return d.toISOString().slice(0, 10);
  }

  loadCollections() {
    this.loading = true;
  const date = this.form.value.date ?? this.today();
  this.financeService.getDailyCollections(date).subscribe({
      next: (data: any[]) => {
        this.collections = data;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = err.error?.Message || 'Failed to load collections.';
        this.loading = false;
      }
    });
  }

  exportCsv() {
  const date = this.form.value.date ?? this.today();
  this.financeService.getDailyCollectionsCsv(date).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'daily_collections.csv';
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }
}
