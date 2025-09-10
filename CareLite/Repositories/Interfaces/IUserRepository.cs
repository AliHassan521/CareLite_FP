using CareLite.Models.Domain;


namespace CareLite.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username);
        Task AuditLogAsync(Guid correlationId, int? userId, string action, string description);
        Task<User> CreateUserAsync(User user, string password);

        // New for duplicate check
        Task<List<User>> FindPotentialDuplicatesAsync(string? fullName, string? email, string? phone);

        // New for search and pagination
        Task<(List<User> Users, int TotalCount)> SearchUsersAsync(string? query, int page, int pageSize);
    }
}
