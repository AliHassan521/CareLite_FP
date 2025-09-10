using CareLite.Models.Domain;
using CareLite.Repositories.Interfaces;
using CareLite.Services.Interfaces;

namespace CareLite.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repo;
        public AuditLogService(IAuditLogRepository repo)
        {
            _repo = repo;
        }
        public async Task<List<AuditLog>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }
        public async Task AddAsync(AuditLog log)
        {
            await _repo.AddAsync(log);
        }
    }
}
