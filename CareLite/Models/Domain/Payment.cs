using System;

namespace CareLite.Models.Domain
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int BillId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } // "Cash" or "Card"
        public int PostedByUserId { get; set; }
        public DateTime PostedAt { get; set; }
    }
}
