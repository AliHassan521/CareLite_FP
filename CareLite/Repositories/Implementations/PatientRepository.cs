using CareLite.Data;
using CareLite.Models.Domain;
using CareLite.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CareLite.Repositories.Implementations
{
    public class PatientRepository : IPatientRepository
    {
        private readonly DbManager _dbManager;
        public PatientRepository(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<Patient> CreatePatientAsync(Patient patient)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_CreatePatient", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@FullName", patient.FullName);
            cmd.Parameters.AddWithValue("@Email", patient.Email);
            cmd.Parameters.AddWithValue("@Phone", patient.Phone);
            cmd.Parameters.AddWithValue("@DateOfBirth", patient.DateOfBirth);
            cmd.Parameters.AddWithValue("@Gender", patient.Gender);
            cmd.Parameters.AddWithValue("@Address", patient.Address);

            // Add output parameter if you want the generated PatientId
            var patientIdParam = new SqlParameter("@PatientId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(patientIdParam);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            // Set the generated PatientId
            patient.PatientId = (int)patientIdParam.Value;
            patient.CreatedAt = DateTime.UtcNow;

            return patient;
        }


        public async Task<List<Patient>> FindPatientDuplicatesAsync(string? fullName, string? email, string? phone)
        {
            var results = new List<Patient>();
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_FindPatientDuplicates", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FullName", (object?)fullName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object?)email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object?)phone ?? DBNull.Value);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new Patient
                {
                    PatientId = (int)reader["PatientId"],
                    FullName = reader["FullName"].ToString(),
                    Email = reader["Email"].ToString(),
                    Phone = reader["Phone"].ToString(),
                    DateOfBirth = (DateTime)reader["DateOfBirth"],
                    Gender = reader["Gender"].ToString(),
                    Address = reader["Address"].ToString(),
                    CreatedAt = (DateTime)reader["CreatedAt"]
                });
            }
            return results;
        }

        public async Task<(List<Patient> Patients, int TotalCount)> SearchPatientsAsync(string? query, int page, int pageSize)
        {
            var patients = new List<Patient>();
            int totalCount = 0;
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_SearchPatients", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Query", (object?)query ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Page", page);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                patients.Add(new Patient
                {
                    PatientId = (int)reader["PatientId"],
                    FullName = reader["FullName"].ToString(),
                    Email = reader["Email"].ToString(),
                    Phone = reader["Phone"].ToString(),
                    DateOfBirth = (DateTime)reader["DateOfBirth"],
                    Gender = reader["Gender"].ToString(),
                    Address = reader["Address"].ToString(),
                    CreatedAt = (DateTime)reader["CreatedAt"]
                });
            }

            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                totalCount = (int)reader["TotalCount"];
            }

            return (patients, totalCount);
        }

        public async Task<Patient> GetPatientByIdAsync(int patientId)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_GetPatientById", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PatientId", patientId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows) return null;
            await reader.ReadAsync();

            return new Patient
            {
                PatientId = (int)reader["PatientId"],
                FullName = reader["FullName"].ToString(),
                Email = reader["Email"].ToString(),
                Phone = reader["Phone"].ToString(),
                DateOfBirth = (DateTime)reader["DateOfBirth"],
                Gender = reader["Gender"].ToString(),
                Address = reader["Address"].ToString(),
                CreatedAt = (DateTime)reader["CreatedAt"]
            };
        }
    }
}
