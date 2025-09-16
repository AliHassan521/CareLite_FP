using System;
using System.Collections.Generic;

namespace CareLite.Models.DTO
{
    public class BillDto
    {
        public int BillId { get; set; }
        public int VisitId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsFinalized { get; set; }
        public List<BillLineItemDto> LineItems { get; set; }
    }

    public class BillLineItemDto
    {
        public int BillLineItemId { get; set; }
        public int BillId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
}
