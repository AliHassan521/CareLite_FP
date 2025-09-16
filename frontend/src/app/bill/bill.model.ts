export interface Bill {
  billId: number;
  visitId: number;
  createdAt: string;
  totalAmount: number;
  isFinalized: boolean;
  lineItems: BillLineItem[];
}

export interface BillLineItem {
  billLineItemId: number;
  billId: number;
  description: string;
  amount: number;
}
