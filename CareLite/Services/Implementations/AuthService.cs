using CareLite.Models.DTO;
using CareLite.Repositories.Interfaces;
using CareLite.Services.Interfaces;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;

namespace CareLite.Services.Implementations
{
    public class AuthService : IAuthService
    {
    private readonly IUserRepository _userRepository;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

        public AuthService(IUserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;
            _jwtSecret = config["Jwt:Key"];
            _jwtIssuer = config["Jwt:Issuer"];
            _jwtAudience = config["Jwt:Audience"];
        }

    public async Task<string?> RegisterAsync(RegisterRequest request, Guid correlationId)
        {
            
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                await _userRepository.AuditLogAsync(correlationId, null, "Register Failed", $"Username {request.Username} already exists");
                throw new UnauthorizedAccessException("Username already exists");
            }

            
            int roleId = request.RoleName?.ToLower() switch
            {
                "admin" => 1,
                "staff" => 2,
                "clinician" => 3,
                _ => 2
            };

            var user = new CareLite.Models.Domain.User
            {
                Username = request.Username,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                RoleId = roleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateUserAsync(user, request.Password);
            if (createdUser == null)
            {
                await _userRepository.AuditLogAsync(correlationId, null, "Register Failed", $"Failed to create user {request.Username}");
                throw new Exception("Failed to create user");
            }

            await _userRepository.AuditLogAsync(correlationId, createdUser.UserId, "Register Success", "User registered successfully");
            
            return GenerateJwtToken(createdUser);
        }

    public async Task<string> LoginAsync(LoginRequest request, Guid correlationId)
        {

            var user = await _userRepository.GetByUsernameAsync(request.Username);

            if(user == null || !user.IsActive)
            {
                await _userRepository.AuditLogAsync(correlationId, null, "Login Failed", $"Invalid login for {request.Username}");
                throw new UnauthorizedAccessException("Invalid Credentials");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                await _userRepository.AuditLogAsync(correlationId, user.UserId, "Login Failed", "Wrong Password");
                throw new UnauthorizedAccessException("Invalid Credentials");
            }

            var token = GenerateJwtToken(user);

            await _userRepository.AuditLogAsync(correlationId, user.UserId, "Login Success", "User logged in successfully");

            return token;
        }

        private string GenerateJwtToken(CareLite.Models.Domain.User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

}

