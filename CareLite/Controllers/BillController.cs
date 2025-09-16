using CareLite.Models.DTO;
using CareLite.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CareLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillController : ControllerBase
    {
        private readonly BillService _billService;
        public BillController(BillService billService)
        {
            _billService = billService;
        }

        [HttpGet("by-visit/{visitId}")]
        [Authorize(Roles = "Billing,Admin,Staff,Clinician")]
        public async Task<IActionResult> GetOrGenerateBill(int visitId)
        {
            var (bill, lineItems) = await _billService.GenerateOrGetBillAsync(visitId);
            if (bill == null)
                return NotFound(new { Message = "No bill found or generated." });
            var dto = new BillDto
            {
                BillId = bill.BillId,
                VisitId = bill.VisitId,
                CreatedAt = bill.CreatedAt,
                TotalAmount = bill.TotalAmount,
                IsFinalized = bill.IsFinalized,
                LineItems = lineItems.Select(li => new BillLineItemDto
                {
                    BillLineItemId = li.BillLineItemId,
                    BillId = li.BillId,
                    Description = li.Description,
                    Amount = li.Amount
                }).ToList()
            };
            return Ok(new { Data = dto });
        }
    }
}
