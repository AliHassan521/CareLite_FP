using CareLite.Data;
using CareLite.Models.Domain;
using CareLite.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CareLite.Repositories.Implementations
{
    public class AppointmentRepository : IAppointmentRepository
        public async Task<List<AppointmentStatusHistory>> GetAppointmentStatusHistoryAsync(int appointmentId)
        {
            var history = new List<AppointmentStatusHistory>();
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("SELECT * FROM AppointmentStatusHistory WHERE AppointmentId = @AppointmentId ORDER BY ChangedAt ASC", conn);
            cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                history.Add(new AppointmentStatusHistory
                {
                    HistoryId = (int)reader["HistoryId"],
                    AppointmentId = (int)reader["AppointmentId"],
                    OldStatus = reader["OldStatus"] == DBNull.Value ? null : reader["OldStatus"].ToString(),
                    NewStatus = reader["NewStatus"].ToString(),
                    ChangedAt = (DateTime)reader["ChangedAt"],
                    ChangedBy = reader["ChangedBy"] == DBNull.Value ? null : (int?)reader["ChangedBy"]
                });
            }
            return history;
        }
        public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment, string newStatus, int? changedBy)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_UpdateAppointment", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AppointmentId", appointment.AppointmentId);
            cmd.Parameters.AddWithValue("@PatientId", appointment.PatientId);
            cmd.Parameters.AddWithValue("@ProviderId", appointment.ProviderId);
            cmd.Parameters.AddWithValue("@StartTime", appointment.StartTime);
            cmd.Parameters.AddWithValue("@DurationMinutes", appointment.DurationMinutes);
            cmd.Parameters.AddWithValue("@NewStatus", newStatus);
            cmd.Parameters.AddWithValue("@ChangedBy", (object?)changedBy ?? DBNull.Value);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Appointment
                {
                    AppointmentId = (int)reader["AppointmentId"],
                    PatientId = (int)reader["PatientId"],
                    ProviderId = (int)reader["ProviderId"],
                    StartTime = (DateTime)reader["StartTime"],
                    DurationMinutes = (int)reader["DurationMinutes"],
                    Status = reader["Status"].ToString(),
                    CreatedAt = (DateTime)reader["CreatedAt"],
                    UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? null : (DateTime?)reader["UpdatedAt"]
                };
            }
            throw new Exception("Failed to update appointment");
        }
    {
        private readonly DbManager _dbManager;
        public AppointmentRepository(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment, int? createdBy)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_CreateAppointment", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PatientId", appointment.PatientId);
            cmd.Parameters.AddWithValue("@ProviderId", appointment.ProviderId);
            cmd.Parameters.AddWithValue("@StartTime", appointment.StartTime);
            cmd.Parameters.AddWithValue("@DurationMinutes", appointment.DurationMinutes);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)createdBy ?? DBNull.Value);
            var appointmentIdParam = new SqlParameter("@AppointmentId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(appointmentIdParam);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            appointment.AppointmentId = (int)appointmentIdParam.Value;
            appointment.Status = "Scheduled";
            appointment.CreatedAt = DateTime.UtcNow;
            return appointment;
        }

        public async Task<List<Appointment>> GetProviderAppointmentsAsync(int providerId, DateTime weekStart, DateTime weekEnd)
        {
            var appointments = new List<Appointment>();
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_GetProviderAppointments", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ProviderId", providerId);
            cmd.Parameters.AddWithValue("@WeekStart", weekStart);
            cmd.Parameters.AddWithValue("@WeekEnd", weekEnd);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                appointments.Add(new Appointment
                {
                    AppointmentId = (int)reader["AppointmentId"],
                    PatientId = (int)reader["PatientId"],
                    PatientName = reader["PatientName"].ToString(),
                    ProviderId = (int)reader["ProviderId"],
                    StartTime = (DateTime)reader["StartTime"],
                    DurationMinutes = (int)reader["DurationMinutes"],
                    Status = reader["Status"].ToString(),
                    CreatedAt = (DateTime)reader["CreatedAt"],
                    UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? null : (DateTime?)reader["UpdatedAt"]
                });;
            }
            return appointments;
        }
    }
}
