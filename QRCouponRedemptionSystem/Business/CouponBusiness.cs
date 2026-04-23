using Dapper;
using QRCouponRedemptionSystem.Controllers;
using QRCouponRedemptionSystem.Model;
using System.Data;
using System.Transactions;

namespace QRCouponRedemptionSystem.Business
{
    public class CouponBusiness : ICouponBusiness
    {
        private readonly DapperContext _db;

        public CouponBusiness(DapperContext db)
        {
            _db = db;
        }

        public async Task<object> RedeemAsync(string userId, string couponCode, string idempotencyKey)
        {
            using var connection = _db.CreateConnection();
            connection.Open();
            try
            {
                  using var transaction = connection.BeginTransaction();
                try
                {
                    var existingTxn = await connection.QueryFirstOrDefaultAsync<Transactions>(
                        @"SELECT * FROM Transactions WITH (UPDLOCK, HOLDLOCK)
              WHERE IdempotencyKey = @Key",
                        new { Key = idempotencyKey },
                        transaction);

                    if (existingTxn != null)
                    {
                        return new
                        {
                            success = true,
                            message = "Already processed",
                            transactionId = existingTxn.Id,
                            status = existingTxn.Status
                        };
                    }

                    var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>(
                        @"SELECT * FROM Coupons WITH (UPDLOCK, ROWLOCK)
              WHERE Code = @Code",
                        new { Code = couponCode },
                        transaction);

                    if (coupon == null)
                        return new { success = false, message = "Invalid coupon" };

                    if (coupon.IsRedeemed)
                        return new { success = false, message = "Coupon already redeemed" };

                    if (coupon.ExpiryDate < DateTime.UtcNow)
                        return new { success = false, message = "Coupon expired" };

                    var wallet = await connection.QueryFirstOrDefaultAsync<Wallet>(
                        @"SELECT * FROM Wallets WITH (UPDLOCK, ROWLOCK)
                               WHERE UserId = @UserId",
                        new { UserId = userId },
                        transaction);

                    if (wallet == null)
                    { var WalletId = Guid.NewGuid().ToString();
                        await connection.ExecuteAsync(
                            @"INSERT INTO Wallets (Id, UserId, Balance) 
                                   VALUES (@Id,@UserId, 0)",
                            new {
                                Id = WalletId,
                                UserId = userId },
                                transaction
                                );
                    }
                    var Id = Guid.NewGuid().ToString();
                    var txnId = await connection.ExecuteScalarAsync<int>(
                        @"INSERT INTO Transactions 
              (Id,UserId, CouponId, Amount, Status, IdempotencyKey, CreatedAt)
              VALUES (@Id, @UserId, @CouponId, @Amount, 'Pending', @Key, GETUTCDATE());
              SELECT CAST(SCOPE_IDENTITY() as int);",
                        new
                        {
                            Id= Id,
                            UserId = userId,
                            CouponId = coupon.Id, 
                            Amount = coupon.Amount,
                            Key = idempotencyKey
                        },
                        transaction);

                    await connection.ExecuteAsync(
                        @"UPDATE Wallets 
              SET Balance = Balance + @Amount
              WHERE UserId = @UserId",
                        new { Amount = coupon.Amount, UserId = userId },
                        transaction);

                    await connection.ExecuteAsync(
                        @"UPDATE Coupons 
              SET IsRedeemed = 1,
                  RedeemedBy = @UserId,
                  RedeemedAt = GETUTCDATE()
              WHERE Id = @CouponId",
                        new { UserId = userId, CouponId = coupon.Id },
                        transaction);

                    await connection.ExecuteAsync(
                        @"UPDATE Transactions 
              SET Status = 'Success'
              WHERE Id = @TxnId",
                        new { TxnId = txnId },
                        transaction);

                    transaction.Commit();

                    return new
                    {
                        success = true,
                        message = "Coupon redeemed successfully",
                        transactionId = txnId
                    };
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    return new
                    {
                        success = false,
                        message = ex.Message
                    };
                }
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    message = ex.Message
                };
            }
        }

        public async Task<decimal> GetWalletBalanceAsync(string  userId)
        {
            using var connection = _db.CreateConnection();
            connection.Open();

            var balance = await connection.QueryFirstOrDefaultAsync<decimal?>(
                @"SELECT Balance 
                  FROM Wallets 
                  WHERE UserId = @UserId",
                new { UserId = userId });

            if (balance == null)
                return 0; 

            return balance.Value;
        }
    }
}