using CareLite.Models.Domain;
using CareLite.Models.DTO;
using CareLite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CareLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitController : ControllerBase
    {
        private readonly IVisitService _visitService;
        public VisitController(IVisitService visitService)
        {
            _visitService = visitService;
        }

        [HttpPost]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> Create([FromBody] VisitDto dto)
        {
            try
            {
               
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type.EndsWith("nameidentifier"));
                int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                if (userId == 0)
                    return BadRequest(new { Message = "Unable to determine clinician from token." });

                var visit = new Visit
                {
                    AppointmentId = dto.AppointmentId,
                    ClinicianId = userId, 
                    Notes = dto.Notes
                };
                var created = await _visitService.CreateVisitAsync(visit);
                return Ok(new { Data = created });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("by-appointment/{appointmentId}")]
        [Authorize(Roles = "Clinician,Admin,Staff")]
        public async Task<IActionResult> GetByAppointment(int appointmentId)
        {
            var visit = await _visitService.GetVisitByAppointmentAsync(appointmentId);
            if (visit == null)
                return NotFound();
            return Ok(new { Data = visit });
        }

        [HttpPut]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> Update([FromBody] VisitDto dto)
        {
            try
            {
             
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type.EndsWith("nameidentifier"));
                int userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                if (userId == 0)
                    return BadRequest(new { Message = "Unable to determine clinician from token." });

                var visit = new Visit
                {
                    VisitId = dto.VisitId,
                    AppointmentId = dto.AppointmentId,
                    ClinicianId = userId, 
                    Notes = dto.Notes,
                    IsFinalized = dto.IsFinalized
                };
                var updated = await _visitService.UpdateVisitAsync(visit);
                return Ok(new { Data = updated });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
