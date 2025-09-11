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
    {
        private readonly DbManager _dbManager;
        public AppointmentRepository(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_CreateAppointment", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PatientId", appointment.PatientId);
            cmd.Parameters.AddWithValue("@ProviderId", appointment.ProviderId);
            cmd.Parameters.AddWithValue("@StartTime", appointment.StartTime);
            cmd.Parameters.AddWithValue("@DurationMinutes", appointment.DurationMinutes);
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
