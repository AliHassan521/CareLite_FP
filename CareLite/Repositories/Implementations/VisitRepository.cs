using CareLite.Data;
using CareLite.Models.Domain;
using CareLite.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace CareLite.Repositories.Implementations
{
    public class VisitRepository : IVisitRepository
    {
        private readonly DbManager _dbManager;
        public VisitRepository(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<Visit> CreateVisitAsync(Visit visit)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_CreateVisit", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AppointmentId", visit.AppointmentId);
            cmd.Parameters.AddWithValue("@ClinicianId", visit.ClinicianId);
            cmd.Parameters.AddWithValue("@Notes", visit.Notes ?? (object)DBNull.Value);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapVisit(reader);
            }
            throw new Exception("Failed to create visit");
        }

        public async Task<Visit> GetVisitByAppointmentAsync(int appointmentId)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_GetVisitByAppointment", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapVisit(reader);
            }
            return null;
        }

        public async Task<Visit> UpdateVisitAsync(Visit visit)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_UpdateVisit", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@VisitId", visit.VisitId);
            cmd.Parameters.AddWithValue("@Notes", visit.Notes ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IsFinalized", visit.IsFinalized);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapVisit(reader);
            }
            throw new Exception("Failed to update visit");
        }

        private Visit MapVisit(SqlDataReader reader)
        {
            return new Visit
            {
                VisitId = (int)reader["VisitId"],
                AppointmentId = (int)reader["AppointmentId"],
                ClinicianId = (int)reader["ClinicianId"],
                Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"].ToString(),
                CreatedAt = (DateTime)reader["CreatedAt"],
                UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? null : (DateTime?)reader["UpdatedAt"],
                IsFinalized = (bool)reader["IsFinalized"]
            };
        }
    }
}
