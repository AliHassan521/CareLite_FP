using CareLite.Models.DTO;
using CareLite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace CareLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        //[AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Guid correlationId = Guid.NewGuid();
            try
            {
                var token = await _authService.LoginAsync(request, correlationId);
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return Ok(new { Token = token, CorrelationId = correlationId });
            }
            catch (UnauthorizedAccessException ex)
            {
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return Unauthorized(new { Message = ex.Message, CorrelationId = correlationId });
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message, CorrelationId = correlationId });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            Guid correlationId = Guid.NewGuid();
            try
            {
                await _authService.RegisterAsync(request, correlationId);
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return Ok(new {
                    Message = "Registration successful.",
                    Username = request.Username,
                    Role = request.RoleName,
                    CorrelationId = correlationId
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return Conflict(new { Message = ex.Message, CorrelationId = correlationId });
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message, CorrelationId = correlationId });
            }
        }
    }
}
