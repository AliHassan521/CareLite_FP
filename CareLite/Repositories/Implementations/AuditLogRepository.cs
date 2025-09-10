using CareLite.Data;
using CareLite.Models.Domain;
using CareLite.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CareLite.Repositories.Implementations
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly DbManager _dbManager;
        public AuditLogRepository(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<List<AuditLog>> GetAllAsync()
        {
            var logs = new List<AuditLog>();
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_GetAuditLogs", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                logs.Add(new AuditLog
                {
                    AuditId = (int)reader["AuditId"],
                    CorrelationId = (Guid)reader["CorrelationId"],
                    UserId = reader["UserId"] == DBNull.Value ? null : (int?)reader["UserId"],
                    Action = reader["Action"].ToString(),
                    Description = reader["Description"].ToString(),
                    CreatedAt = (DateTime)reader["CreatedAt"]
                });
            }
            return logs;
        }
        public async Task AddAsync(AuditLog log)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_AddAuditLog", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@CorrelationId", log.CorrelationId);
            cmd.Parameters.AddWithValue("@UserId", (object?)log.UserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Action", log.Action);
            cmd.Parameters.AddWithValue("@Description", log.Description);
            cmd.Parameters.AddWithValue("@CreatedAt", log.CreatedAt);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

    }
}
