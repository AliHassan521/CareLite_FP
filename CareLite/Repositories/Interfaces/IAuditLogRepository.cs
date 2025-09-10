using CareLite.Models.Domain;

namespace CareLite.Repositories.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<List<AuditLog>> GetAllAsync();
        Task AddAsync(AuditLog log);
    }
}
