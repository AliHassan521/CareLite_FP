export interface Payment {
  paymentId: number;
  billId: number;
  amount: number;
  method: 'Cash' | 'Card';
  postedByUserId: number;
  postedAt: string;
}
