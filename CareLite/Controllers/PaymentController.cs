using CareLite.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CareLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("record")]
        [Authorize(Roles = "Billing,Admin,Staff")]
        public async Task<IActionResult> RecordPayment(int billId, decimal amount, string method)
        {
            // Get userId from token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type.EndsWith("nameidentifier"));
            int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            if (userId == 0)
                return BadRequest(new { Message = "Unable to determine user from token." });

            try
            {
                var (totalAmount, remainingBalance) = await _paymentService.RecordPaymentAsync(billId, amount, method, userId);
                return Ok(new { TotalAmount = totalAmount, RemainingBalance = remainingBalance });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
