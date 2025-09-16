
using CareLite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CareLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;
        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        [HttpGet("outstanding-balances")]
        [Authorize(Roles = "Finance,Admin,Staff")]
        public async Task<IActionResult> GetOutstandingBalances([FromQuery] string? patientName, [FromQuery] bool csv = false)
        {
            var rows = await _financeService.GetOutstandingBalancesAsync(patientName);
            if (csv)
            {
                var sb = new StringBuilder();
                sb.AppendLine("PatientId,FullName,BillId,TotalAmount,TotalPaid,OutstandingBalance");
                foreach (dynamic r in rows)
                {
                    sb.AppendLine($"{r.PatientId},{r.FullName},{r.BillId},{r.TotalAmount},{r.TotalPaid},{r.OutstandingBalance}");
                }
                return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "outstanding_balances.csv");
            }
            return Ok(new { Data = rows });
        }

        [HttpGet("daily-collections")]
        [Authorize(Roles = "Finance,Admin,Staff")]
        public async Task<IActionResult> GetDailyCollections([FromQuery] DateTime date, [FromQuery] bool csv = false)
        {
            var rows = await _financeService.GetDailyCollectionsAsync(date);
            if (csv)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Method,TotalCollected");
                foreach (dynamic r in rows)
                {
                    sb.AppendLine($"{r.Method},{r.TotalCollected}");
                }
                return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "daily_collections.csv");
            }
            return Ok(new { Data = rows });
        }
    }
}
