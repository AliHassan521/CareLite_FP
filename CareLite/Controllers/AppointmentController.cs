using CareLite.Models.Domain;
using CareLite.Models.DTO;
using CareLite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var startTime = request.StartTime; // DateTime from request
            var clinicStart = new TimeSpan(9, 0, 0);
            var clinicEnd = new TimeSpan(17, 0, 0);
            var breakStart = new TimeSpan(13, 0, 0);
            var breakEnd = new TimeSpan(14, 0, 0);

            if (startTime.TimeOfDay < clinicStart || startTime.TimeOfDay >= clinicEnd)
            {
                return BadRequest(new { Message = "Appointment must be within business hours (09:00–17:00)." });
            }

            if (startTime.TimeOfDay >= breakStart && startTime.TimeOfDay < breakEnd)
            {
                return BadRequest(new { Message = "Cannot schedule during break time (13:00–14:00)." });
            }

            try
            {
                var appointment = new Appointment
                {
                    PatientId = request.PatientId,
                    ProviderId = request.ProviderId,
                    StartTime = request.StartTime,
                    DurationMinutes = request.DurationMinutes
                };
                var created = await _appointmentService.ScheduleAppointmentAsync(appointment);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("provider/{providerId}")]
        [Authorize(Roles = "Admin,Staff,Clinician")]
        public async Task<IActionResult> GetProviderAppointments(int providerId, [FromQuery] DateTime weekStart, [FromQuery] DateTime weekEnd)
        {
            var appointments = await _appointmentService.GetProviderAppointmentsAsync(providerId, weekStart, weekEnd);
            return Ok(appointments);
        }
    }
}
