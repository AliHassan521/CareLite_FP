using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace CareLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinanceController : ControllerBase
    {
        private readonly string _connStr;
        public FinanceController()
        {
            // TODO: Use DI for DbManager, here for demo
            _connStr = Environment.GetEnvironmentVariable("CARE_LITE_CONN") ?? "";
        }

        [HttpGet("outstanding-balances")]
        [Authorize(Roles = "Finance,Admin,Staff")]
        public async Task<IActionResult> GetOutstandingBalances([FromQuery] string? patientName, [FromQuery] bool csv = false)
        {
            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand("sp_ReportOutstandingBalances", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PatientName", (object?)patientName ?? DBNull.Value);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var rows = new List<dynamic>();
            while (await reader.ReadAsync())
            {
                rows.Add(new {
                    PatientId = reader["PatientId"],
                    FullName = reader["FullName"],
                    BillId = reader["BillId"],
                    TotalAmount = reader["TotalAmount"],
                    TotalPaid = reader["TotalPaid"],
                    OutstandingBalance = reader["OutstandingBalance"]
                });
            }
            if (csv)
            {
                var sb = new StringBuilder();
                sb.AppendLine("PatientId,FullName,BillId,TotalAmount,TotalPaid,OutstandingBalance");
                foreach (var r in rows)
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
            using var conn = new SqlConnection(_connStr);
            using var cmd = new SqlCommand("sp_ReportDailyCollections", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Date", date.Date);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var rows = new List<dynamic>();
            while (await reader.ReadAsync())
            {
                rows.Add(new {
                    Method = reader["Method"],
                    TotalCollected = reader["TotalCollected"]
                });
            }
            if (csv)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Method,TotalCollected");
                foreach (var r in rows)
                {
                    sb.AppendLine($"{r.Method},{r.TotalCollected}");
                }
                return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "daily_collections.csv");
            }
            return Ok(new { Data = rows });
        }
    }
}
