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
    [Authorize(Roles = "Admin,Staff")]
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
        public async Task<IActionResult> Create([FromBody] CreatePatientRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check for duplicates before creation
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

            // Add audit log
            await _auditLogService.AddAsync(new AuditLog
            {
                CorrelationId = Guid.NewGuid(),
                UserId = int.TryParse(User.Identity?.Name, out var userId) ? userId : null,
                Action = "Create Patient",
                Description = $"Patient {created.FullName} created with ID {created.PatientId}",
                CreatedAt = DateTime.UtcNow
            });

            return Ok(created);
        }

        [HttpPost("check-duplicates")]
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
