using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly string _connectionString;
        public SettingsController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        [HttpGet("business-hours")]
        [Authorize(Roles = "Admin,Staff,Clinician")]
        public async Task<IActionResult> GetBusinessHours()
        {
            var result = new Dictionary<string, string>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_GetBusinessHours", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result[reader["SettingKey"].ToString()] = reader["SettingValue"].ToString();
            }
            return Ok(result);
        }
    }
}
