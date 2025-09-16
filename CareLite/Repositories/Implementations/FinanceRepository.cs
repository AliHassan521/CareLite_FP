using CareLite.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using CareLite.Data;

namespace CareLite.Repositories.Implementations
{
    public class FinanceRepository : IFinanceRepository
    {
        private readonly DbManager _dbManager;
        public FinanceRepository(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<List<object>> GetOutstandingBalancesAsync(string patientName = null)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_ReportOutstandingBalances", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PatientName", (object?)patientName ?? DBNull.Value);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var rows = new List<object>();
            while (await reader.ReadAsync())
            {
                rows.Add(new {
                    PatientId = reader["PatientId"],
                    FullName = reader["FullName"],
                    BillId = reader["BillId"],
                    TotalAmount = reader["TotalAmount"],
                    TotalPaid = reader["TotalPaid"],
                    OutstandingBalance = reader["OutstandingBalance"]
                });
            }
            return rows;
        }

        public async Task<List<object>> GetDailyCollectionsAsync(DateTime date)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_ReportDailyCollections", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Date", date.Date);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            var rows = new List<object>();
            while (await reader.ReadAsync())
            {
                rows.Add(new {
                    Method = reader["Method"],
                    TotalCollected = reader["TotalCollected"]
                });
            }
            return rows;
        }
    }
}
