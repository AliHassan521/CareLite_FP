using CareLite.Models.DTO;

namespace CareLite.Services.Interfaces
{
    public interface IAuthService
    {
    Task<string> LoginAsync(LoginRequest request, Guid correlationId);
    Task<string?> RegisterAsync(RegisterRequest request, Guid correlationId);
    }
}