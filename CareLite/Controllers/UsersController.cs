using CareLite.Models.Domain;
using CareLite.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CareLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: api/users?role=Clinician
        [HttpGet]
        [Authorize(Roles = "Admin,Staff,Clinician")]
        public async Task<IActionResult> GetUsers([FromQuery] string? role = null)
        {
            var correlationId = Guid.NewGuid();
            try
            {
                // For now, fetch all users and filter by role name in memory
                var (users, _) = await _userRepository.SearchUsersAsync(null, 1, 1000);
                if (!string.IsNullOrEmpty(role))
                {
                    users = users.Where(u => u.Role != null && u.Role.RoleName == role).ToList();
                }
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return Ok(new {
                    Data = users.Select(u => new {
                        userId = u.UserId,
                        fullName = u.FullName,
                        email = u.Email,
                        role = u.Role?.RoleName
                    }),
                    CorrelationId = correlationId
                });
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message, CorrelationId = correlationId });
            }
        }
    }
}
