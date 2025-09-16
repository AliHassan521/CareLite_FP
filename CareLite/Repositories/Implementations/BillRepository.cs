using CareLite.Models.Domain;
using CareLite.Data;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace CareLite.Repositories.Interfaces
{
    public class BillRepository : IBillRepository
    {
        private readonly DbManager _dbManager;
        public BillRepository(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<(Bill, List<BillLineItem>)> GenerateOrGetBillAsync(int visitId)
        {
            using var conn = _dbManager.GetConnection();
            using var cmd = new SqlCommand("sp_GenerateOrGetBill", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@VisitId", visitId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            Bill bill = null;
            var lineItems = new List<BillLineItem>();
            // First result: Bill
            if (await reader.ReadAsync())
            {
                bill = MapBill(reader);
            }
            // Next result: BillLineItems
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    lineItems.Add(MapLineItem(reader));
                }
            }
            return (bill, lineItems);
        }

        private Bill MapBill(SqlDataReader reader)
        {
            return new Bill
            {
                BillId = (int)reader["BillId"],
                VisitId = (int)reader["VisitId"],
                CreatedAt = (DateTime)reader["CreatedAt"],
                TotalAmount = (decimal)reader["TotalAmount"],
                IsFinalized = (bool)reader["IsFinalized"]
            };
        }

        private BillLineItem MapLineItem(SqlDataReader reader)
        {
            return new BillLineItem
            {
                BillLineItemId = (int)reader["BillLineItemId"],
                BillId = (int)reader["BillId"],
                Description = reader["Description"].ToString(),
                Amount = (decimal)reader["Amount"]
            };
        }
    }
}
