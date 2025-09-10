
using Microsoft.Data.SqlClient;

namespace CareLite.Data
{
    public class DbManager
    {
        private readonly string _connectionString;

        public DbManager(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}