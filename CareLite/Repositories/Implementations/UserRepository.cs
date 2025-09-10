using CareLite.Data;
using CareLite.Models.Domain;
using CareLite.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;


namespace CareLite.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly DbManager _dbManager;
        
        public UserRepository(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_GetUserByUsername", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Username", username);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows) return null;

            await reader.ReadAsync();

            return new User
            {
                UserId = (int)reader["UserId"],
                Username = reader["Username"].ToString(),
                PasswordHash = reader["PasswordHash"].ToString(),
                FullName = reader["FullName"].ToString(),
                Email = reader["Email"].ToString(),
                Phone = reader["Phone"].ToString(),
                RoleId = (int)reader["RoleId"],
                Role = new Role
                {
                    RoleId = (int)reader["RoleId"],
                    RoleName = reader["RoleName"].ToString()
                },
                IsActive = (bool)reader["IsActive"],
                // CreatedAt = (DateTime)reader["CreatedAt"],
                // UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? null : (DateTime?)reader["UpdatedAt"]
            };
        }

        public async Task AuditLogAsync(Guid correlationId, int? userId, string action, string description)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_InsertAuditLog", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@CorrelationId", correlationId);
            cmd.Parameters.AddWithValue("@UserId", (object)userId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("Action", action);
            cmd.Parameters.AddWithValue("@Description", description);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task<User> CreateUserAsync(User user, string password)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_CreateUser", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            cmd.Parameters.AddWithValue("@Username", user.Username);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
            cmd.Parameters.AddWithValue("@FullName", user.FullName);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@Phone", user.Phone);
            cmd.Parameters.AddWithValue("@RoleId", user.RoleId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows) return null;
            await reader.ReadAsync();
            return new User
            {
                UserId = (int)reader["UserId"],
                Username = reader["Username"].ToString(),
                PasswordHash = reader["PasswordHash"].ToString(),
                FullName = reader["FullName"].ToString(),
                Email = reader["Email"].ToString(),
                Phone = reader["Phone"].ToString(),
                RoleId = (int)reader["RoleId"],
                Role = new Role
                {
                    RoleId = (int)reader["RoleId"],
                    RoleName = reader["RoleName"].ToString()
                },
                IsActive = (bool)reader["IsActive"],
                CreatedAt = (DateTime)reader["CreatedAt"]
            };
        }
        // Find potential duplicates by name, email, or phone
        public async Task<List<User>> FindPotentialDuplicatesAsync(string? fullName, string? email, string? phone)
        {
            var results = new List<User>();
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_FindPotentialDuplicates", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FullName", (object?)fullName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object?)email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object?)phone ?? DBNull.Value);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new User
                {
                    UserId = (int)reader["UserId"],
                    Username = reader["Username"].ToString(),
                    FullName = reader["FullName"].ToString(),
                    Email = reader["Email"].ToString(),
                    Phone = reader["Phone"].ToString(),
                });
            }
            return results;
        }

        // Search users with pagination and case-insensitive search
        public async Task<(List<User> Users, int TotalCount)> SearchUsersAsync(string? query, int page, int pageSize)
        {
            var users = new List<User>();
            int totalCount = 0;
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_SearchUsers", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Query", (object?)query ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Page", page);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    UserId = (int)reader["UserId"],
                    Username = reader["Username"].ToString(),
                    FullName = reader["FullName"].ToString(),
                    Email = reader["Email"].ToString(),
                    Phone = reader["Phone"].ToString(),
                    RoleId = (int)reader["RoleId"],
                    IsActive = (bool)reader["IsActive"],
                    CreatedAt = (DateTime)reader["CreatedAt"]
                });
            }
            // Move to next result set for total count
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                totalCount = (int)reader["TotalCount"];
            }
            return (users, totalCount);
        }
    }
}