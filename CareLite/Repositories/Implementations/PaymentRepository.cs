using CareLite.Models.Domain;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CareLite.Repositories.Implementations
{
    public class PaymentRepository
    {
        private readonly DbManager _dbManager;
        public PaymentRepository(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<(decimal totalAmount, decimal remainingBalance)> RecordPaymentAsync(int billId, decimal amount, string method, int userId)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_RecordPayment", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@BillId", billId);
            cmd.Parameters.AddWithValue("@Amount", amount);
            cmd.Parameters.AddWithValue("@Method", method);
            cmd.Parameters.AddWithValue("@PostedByUserId", userId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var totalAmount = (decimal)reader["TotalAmount"];
                var remainingBalance = (decimal)reader["RemainingBalance"];
                return (totalAmount, remainingBalance);
            }
            throw new Exception("Failed to record payment or fetch balance");
        }
    }
}
