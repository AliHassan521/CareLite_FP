using System;

namespace CareLite.Models.DTO
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int BillId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; }
        public int PostedByUserId { get; set; }
        public DateTime PostedAt { get; set; }
    }
}
