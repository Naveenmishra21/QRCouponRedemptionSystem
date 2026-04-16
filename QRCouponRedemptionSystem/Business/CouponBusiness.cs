using Dapper;
using QRCouponRedemptionSystem.Controllers;
using System.Data;

namespace QRCouponRedemptionSystem.Business
{
    public class CouponBusiness : ICouponBusiness
    {
        private readonly DapperContext _db;

        public CouponBusiness(DapperContext db)
        {
            _db = db;
        }

        public async Task<object> RedeemAsync(int userId, string couponCode, string idempotencyKey)
        {
            using var connection = _db.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // ✅ Idempotency Check
                var existingTxn = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM Transactions WHERE IdempotencyKey = @Key",
                    new { Key = idempotencyKey },
                    transaction);

                if (existingTxn != null)
                {
                    return new
                    {
                        message = "Already processed",
                        status = existingTxn.Status
                    };
                }

                // ✅ Lock Coupon Row
                var coupon = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT * FROM Coupons WITH (UPDLOCK, ROWLOCK)
                      WHERE Code = @Code",
                    new { Code = couponCode },
                    transaction);

                if (coupon == null)
                    throw new Exception("Invalid coupon");

                if (coupon.IsRedeemed)
                    throw new Exception("Coupon already redeemed");

                if (coupon.ExpiryDate < DateTime.UtcNow)
                    throw new Exception("Coupon expired");

                // ✅ Wallet Check (Auto-create if not exists)
                var wallet = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT * FROM Wallets WHERE UserId = @UserId",
                    new { UserId = userId },
                    transaction);

                if (wallet == null)
                {
                    await connection.ExecuteAsync(
                        "INSERT INTO Wallets (UserId, Balance) VALUES (@UserId, 0)",
                        new { UserId = userId },
                        transaction);
                }

                // ✅ Insert Transaction (Pending)
                var txnId = await connection.ExecuteScalarAsync<int>(
                    @"INSERT INTO Transactions 
                      (UserId, CouponId, Amount, Status, IdempotencyKey, CreatedAt)
                      VALUES (@UserId, @CouponId, @Amount, 'Pending', @Key, GETUTCDATE());
                      SELECT CAST(SCOPE_IDENTITY() as int);",
                    new
                    {
                        UserId = userId,
                        CouponId = coupon.Id,
                        Amount = coupon.Amount,
                        Key = idempotencyKey
                    },
                    transaction);

                // ✅ Update Wallet Balance
                await connection.ExecuteAsync(
                    @"UPDATE Wallets 
                      SET Balance = Balance + @Amount
                      WHERE UserId = @UserId",
                    new { Amount = coupon.Amount, UserId = userId },
                    transaction);

                // ✅ Mark Coupon Redeemed
                await connection.ExecuteAsync(
                    @"UPDATE Coupons 
                      SET IsRedeemed = 1,
                          RedeemedBy = @UserId,
                          RedeemedAt = GETUTCDATE()
                      WHERE Id = @CouponId",
                    new { UserId = userId, CouponId = coupon.Id },
                    transaction);

                // ✅ Mark Transaction Success
                await connection.ExecuteAsync(
                    @"UPDATE Transactions 
                      SET Status = 'Success'
                      WHERE Id = @TxnId",
                    new { TxnId = txnId },
                    transaction);

                transaction.Commit();

                return new
                {
                    message = "Coupon redeemed successfully"
                };
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<decimal> GetWalletBalanceAsync(int userId)
        {
            using var connection = _db.CreateConnection();
            connection.Open();

            var balance = await connection.QueryFirstOrDefaultAsync<decimal?>(
                @"SELECT Balance 
                  FROM Wallets 
                  WHERE UserId = @UserId",
                new { UserId = userId });

            if (balance == null)
                return 0; // better UX

            return balance.Value;
        }
    }
}