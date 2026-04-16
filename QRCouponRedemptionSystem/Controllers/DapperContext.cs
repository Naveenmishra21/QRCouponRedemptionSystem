using Microsoft.Data.SqlClient;
using System.Data;

namespace QRCouponRedemptionSystem.Controllers
{
    public class DapperContext
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public DapperContext(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()=> new SqlConnection(_connectionString);
    }
}
