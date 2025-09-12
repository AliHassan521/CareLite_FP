using CareLite.Models.Domain;
using CareLite.Models.DTO;
using CareLite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IBusinessHoursService _businessHoursService;

        public AppointmentController(IAppointmentService appointmentService, IBusinessHoursService businessHoursService)
        {
            _appointmentService = appointmentService;
            _businessHoursService = businessHoursService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
        {
            var correlationId = Guid.NewGuid();
            var startTime = request.StartTime;

            try
            {
                // Fetch business hours and break times from service
                var (clinicStart, clinicEnd) = _businessHoursService.GetBusinessHoursForProvider(request.ProviderId);
                // For demo, break time is hardcoded; replace with real logic if needed
                var breakStart = TimeSpan.FromHours(12);
                var breakEnd = TimeSpan.FromHours(13);

                if (startTime.TimeOfDay < clinicStart || startTime.TimeOfDay >= clinicEnd)
                {
                    Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                    return BadRequest(new { Message = $"Appointment must be within business hours ({clinicStart:hh\\:mm}–{clinicEnd:hh\\:mm}).", CorrelationId = correlationId });
                }

                if (startTime.TimeOfDay >= breakStart && startTime.TimeOfDay < breakEnd)
                {
                    Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                    return BadRequest(new { Message = $"Cannot schedule during break time ({breakStart:hh\\:mm}–{breakEnd:hh\\:mm}).", CorrelationId = correlationId });
                }

                // Check for overlapping appointment
                var overlap = await _appointmentService.GetOverlappingAppointmentAsync(request.ProviderId, request.StartTime, request.DurationMinutes);
                if (overlap != null)
                {
                    Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                    return Conflict(new { Message = $"Conflicts with another appointment: {overlap.StartTime:HH:mm}–{overlap.StartTime.AddMinutes(overlap.DurationMinutes):HH:mm}", Conflict = new { overlap.AppointmentId, overlap.StartTime, overlap.DurationMinutes }, CorrelationId = correlationId });
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
                // Fetch business hours and break times from service
                var (clinicStart, clinicEnd) = _businessHoursService.GetBusinessHoursForProvider(request.ProviderId);
                // For demo, break time is hardcoded; replace with real logic if needed
                var breakStart = TimeSpan.FromHours(12);
                var breakEnd = TimeSpan.FromHours(13);

                if (startTime.TimeOfDay < clinicStart || startTime.TimeOfDay >= clinicEnd)
                {
                    Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                    return BadRequest(new { Message = $"Appointment must be within business hours ({clinicStart:hh\\:mm}–{clinicEnd:hh\\:mm}).", CorrelationId = correlationId });
                }

                if (startTime.TimeOfDay >= breakStart && startTime.TimeOfDay < breakEnd)
                {
                    Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                    return BadRequest(new { Message = $"Cannot schedule during break time ({breakStart:hh\\:mm}–{breakEnd:hh\\:mm}).", CorrelationId = correlationId });
                }

                // Check for overlapping appointment (exclude current appointment)
                var overlap = await _appointmentService.GetOverlappingAppointmentAsync(request.ProviderId, request.StartTime, request.DurationMinutes, request.AppointmentId);
                if (overlap != null)
                {
                    Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                    return Conflict(new { Message = $"Conflicts with another appointment: {overlap.StartTime:HH:mm}–{overlap.StartTime.AddMinutes(overlap.DurationMinutes):HH:mm}", Conflict = new { overlap.AppointmentId, overlap.StartTime, overlap.DurationMinutes }, CorrelationId = correlationId });
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

                var updated = await _appointmentService.UpdateAppointmentAsync(appointment,request.NewStatus, userId);
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return Ok(new { Data = updated, CorrelationId = correlationId });
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return BadRequest(new { Message = ex.Message, CorrelationId = correlationId });
            }
        }

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
    }
}
