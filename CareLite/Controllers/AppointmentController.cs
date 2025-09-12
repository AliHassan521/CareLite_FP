

namespace CareLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
        {
            var correlationId = Guid.NewGuid();
            var startTime = request.StartTime;
            try
            {
                // Fetch business hours and break times from DB
                var httpClient = new System.Net.Http.HttpClient();
                var apiUrl = $"{Request.Scheme}://{Request.Host}/api/settings/business-hours";
                var response = await httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
                var clinicStart = TimeSpan.Parse(dict["ClinicStart"]);
                var clinicEnd = TimeSpan.Parse(dict["ClinicEnd"]);
                var breakStart = TimeSpan.Parse(dict["BreakStart"]);
                var breakEnd = TimeSpan.Parse(dict["BreakEnd"]);

                if (startTime.TimeOfDay < clinicStart || startTime.TimeOfDay >= clinicEnd)
                {
                    Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                    return BadRequest(new { Message = $"Appointment must be within business hours ({clinicStart:hh\:mm}–{clinicEnd:hh\:mm}).", CorrelationId = correlationId });
                }

                if (startTime.TimeOfDay >= breakStart && startTime.TimeOfDay < breakEnd)
                {
                    Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                    return BadRequest(new { Message = $"Cannot schedule during break time ({breakStart:hh\:mm}–{breakEnd:hh\:mm}).", CorrelationId = correlationId });
                }

                var appointment = new Appointment
                {
                    PatientId = request.PatientId,
                    ProviderId = request.ProviderId,
                    StartTime = request.StartTime,
                    DurationMinutes = request.DurationMinutes
                };
                // Get userId from JWT claims
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type.EndsWith("nameidentifier"));
                int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                var created = await _appointmentService.ScheduleAppointmentAsync(appointment, userId);
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return Ok(new { Data = created, CorrelationId = correlationId });
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return BadRequest(new { Message = ex.Message, CorrelationId = correlationId });
            }
        }

        [HttpGet("provider/{providerId}")]
        [Authorize(Roles = "Admin,Staff,Clinician")]
        public async Task<IActionResult> GetProviderAppointments(int providerId, [FromQuery] DateTime weekStart, [FromQuery] DateTime weekEnd)
        {
            var correlationId = Guid.NewGuid();
            try
            {
                var appointments = await _appointmentService.GetProviderAppointmentsAsync(providerId, weekStart, weekEnd);
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return Ok(new { Data = appointments, CorrelationId = correlationId });
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message, CorrelationId = correlationId });
            }
        }
        [HttpPut]
        [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update([FromBody] UpdateAppointmentRequest request)
        {
            var correlationId = Guid.NewGuid();
            var startTime = request.StartTime;
            try
            {
                // Fetch business hours and break times from DB
                var httpClient = new System.Net.Http.HttpClient();
                var apiUrl = $"{Request.Scheme}://{Request.Host}/api/settings/business-hours";
                var response = await httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
                var clinicStart = TimeSpan.Parse(dict["ClinicStart"]);
                var clinicEnd = TimeSpan.Parse(dict["ClinicEnd"]);
                var breakStart = TimeSpan.Parse(dict["BreakStart"]);
                var breakEnd = TimeSpan.Parse(dict["BreakEnd"]);

                if (startTime.TimeOfDay < clinicStart || startTime.TimeOfDay >= clinicEnd)
                {
                    Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                    return BadRequest(new { Message = $"Appointment must be within business hours ({clinicStart:hh\:mm}–{clinicEnd:hh\:mm}).", CorrelationId = correlationId });
                }

                if (startTime.TimeOfDay >= breakStart && startTime.TimeOfDay < breakEnd)
                {
                    Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                    return BadRequest(new { Message = $"Cannot schedule during break time ({breakStart:hh\:mm}–{breakEnd:hh\:mm}).", CorrelationId = correlationId });
                }

                var appointment = new Appointment
                {
                    AppointmentId = request.AppointmentId,
                    PatientId = request.PatientId,
                    ProviderId = request.ProviderId,
                    StartTime = request.StartTime,
                    DurationMinutes = request.DurationMinutes
                };
                // Get userId from JWT claims
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type.EndsWith("nameidentifier"));
                int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                var updated = await _appointmentService.UpdateAppointmentAsync(appointment, userId);
        [HttpGet("{appointmentId}/status-history")]
        [Authorize(Roles = "Admin,Staff,Clinician")]
        public async Task<IActionResult> GetStatusHistory(int appointmentId)
        {
            var correlationId = Guid.NewGuid();
            try
            {
                var history = await _appointmentService.GetAppointmentStatusHistoryAsync(appointmentId);
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return Ok(new { Data = history, CorrelationId = correlationId });
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message, CorrelationId = correlationId });
            }
        }
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return Ok(new { Data = updated, CorrelationId = correlationId });
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return BadRequest(new { Message = ex.Message, CorrelationId = correlationId });
            }
        }
    }
}
