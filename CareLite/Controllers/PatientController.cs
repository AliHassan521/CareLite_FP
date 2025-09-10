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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var duplicates = await _patientRepository.FindPatientDuplicatesAsync(request.FullName, request.Email, request.Phone);
            if (duplicates.Any())
                return Conflict(new { Message = "Potential duplicate found.", Matches = duplicates });

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
                return Conflict(new { Message = "Duplicate patient found." });


            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if(int.TryParse(userIdClaim, out var parseId))
            {
                userId = parseId;
            }

            await _auditLogService.AddAsync(new AuditLog
            {
                CorrelationId = Guid.NewGuid(),
                UserId = userId,
                Action = "Create Patient",
                Description = $"Patient {created.FullName} created with ID {created.PatientId}",
                CreatedAt = DateTime.UtcNow
            });

            return Ok(created);
        }

        [HttpPost("check-duplicates")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CheckDuplicates([FromBody] CreatePatientRequest request)
        {
            var matches = await _patientRepository.FindPatientDuplicatesAsync(request.FullName, request.Email, request.Phone);
            return Ok(matches);
        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin,Staff,Clinician")]
        public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (patients, totalCount) = await _patientRepository.SearchPatientsAsync(query, page, pageSize);
            if (patients == null) patients = new List<Patient>();
            return Ok(new { patients, total = totalCount });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Clinician")]
        public async Task<IActionResult> Get(int id)
        {
            var patient = await _patientRepository.GetPatientByIdAsync(id);
            if (patient == null) return NotFound();
            return Ok(patient);
        }
    }
}
