using CareLite.Models.Domain;
using CareLite.Models.DTO;
using CareLite.Repositories.Interfaces;
using CareLite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IAuditLogService _auditLogService;

        public PatientController(IPatientRepository patientRepository, IAuditLogService auditLogService)
        {
            _patientRepository = patientRepository;
            _auditLogService = auditLogService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] CreatePatientRequest request)
        {
            var correlationId = Guid.NewGuid();
            try
            {
                if (!ModelState.IsValid)
                {
                    Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                    return BadRequest(new { Message = "Validation failed.", Errors = ModelState, CorrelationId = correlationId });
                }

                var duplicates = await _patientRepository.FindPatientDuplicatesAsync(request.FullName, request.Email, request.Phone);
                if (duplicates.Any())
                {
                    Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                    return Conflict(new { Message = "Potential duplicate found.", Matches = duplicates, CorrelationId = correlationId });
                }

                var patient = new Patient
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    Phone = request.Phone,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    Address = request.Address
                };

                var created = await _patientRepository.CreatePatientAsync(patient);
                if (created == null)
                {
                    Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                    return Conflict(new { Message = "Duplicate patient found.", CorrelationId = correlationId });
                }

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int? userId = null;
                if(int.TryParse(userIdClaim, out var parseId))
                {
                    userId = parseId;
                }

                await _auditLogService.AddAsync(new AuditLog
                {
                    CorrelationId = correlationId,
                    UserId = userId,
                    Action = "Create Patient",
                    Description = $"Patient {created.FullName} created with ID {created.PatientId}",
                    CreatedAt = DateTime.UtcNow
                });

                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return Ok(new { Data = created, CorrelationId = correlationId });
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message, CorrelationId = correlationId });
            }
        }

        [HttpPost("check-duplicates")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CheckDuplicates([FromBody] CreatePatientRequest request)
        {
            var correlationId = Guid.NewGuid();
            try
            {
                var matches = await _patientRepository.FindPatientDuplicatesAsync(request.FullName, request.Email, request.Phone);
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return Ok(new { Matches = matches, CorrelationId = correlationId });
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message, CorrelationId = correlationId });
            }
        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin,Staff,Clinician")]
        public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var correlationId = Guid.NewGuid();
            try
            {
                var (patients, totalCount) = await _patientRepository.SearchPatientsAsync(query, page, pageSize);
                if (patients == null) patients = new List<Patient>();
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return Ok(new { patients, total = totalCount, CorrelationId = correlationId });
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message, CorrelationId = correlationId });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Clinician")]
        public async Task<IActionResult> Get(int id)
        {
            var correlationId = Guid.NewGuid();
            try
            {
                var patient = await _patientRepository.GetPatientByIdAsync(id);
                if (patient == null)
                {
                    Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                    return NotFound(new { Message = "Patient not found.", CorrelationId = correlationId });
                }
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return Ok(new { Data = patient, CorrelationId = correlationId });
            }
            catch (Exception ex)
            {
                Response.Headers.Add("X-Correlation-Id", correlationId.ToString());
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message, CorrelationId = correlationId });
            }
        }
    }
}
