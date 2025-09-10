using CareLite.Models.Domain;
using CareLite.Repositories.Interfaces;

namespace CareLite.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<List<AuditLog>> GetAllAsync();
        Task AddAsync(AuditLog log);
    }
}
