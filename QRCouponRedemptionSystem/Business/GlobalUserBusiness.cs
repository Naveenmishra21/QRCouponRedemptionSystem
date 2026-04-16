using Dapper;
using Microsoft.AspNetCore.Mvc;
using QRCouponRedemptionSystem.Controllers;
using QRCouponRedemptionSystem.Model;

namespace QRCouponRedemptionSystem.Business
{
    public class GlobalUserBusiness : IGlobalUserBusiness
    {
        private readonly DapperContext _context;
        public GlobalUserBusiness(DapperContext context)
        {
            _context = context;
        }
        public async Task<Tuple<bool,string>> CheckUserExists(string username, string password)
        {
            using var connection = _context.CreateConnection();

            var storedHash = await connection.ExecuteScalarAsync<string>(
                "SELECT Password FROM Users WHERE Username = @Username",
                new { Username = username });

            if (storedHash == null)
                return Tuple.Create(false, "");

            var isvaild = PasswordHelper.VerifyPassword(password, storedHash);

            return Tuple.Create(isvaild, isvaild ? username : "");
        }

        public async Task<bool> RegisterUser(User dto)
        {
            using var connection = _context.CreateConnection();

            var exists = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Users WHERE Username = @Username OR Email = @Email",
                new { dto.Username, dto.Email });

            if (exists > 0)
                return false;

            var hashedPassword = PasswordHelper.HashPassword(dto.Password);

            var query = @"INSERT INTO Users (Username, Email, Password)
                      VALUES (@Username, @Email, @Password)";

            var result = await connection.ExecuteAsync(query, new
            {
                dto.Username,
                dto.Email,
                Password = hashedPassword
            });

            return result > 0;
        }
    }
}
