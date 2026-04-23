using Dapper;
using QRCouponRedemptionSystem.Controllers;
using QRCouponRedemptionSystem.Model;
using System.Data;

namespace QRCouponRedemptionSystem.Business
{
    public class AdminBusiness : IAdminBusiness
    {
        private readonly DapperContext _context;

        public AdminBusiness(DapperContext context)
        {
            _context = context;
        }

        public async Task<string> CreateCampaignAsync(Campaigns campaign)
        {
            using var connection = _context.CreateConnection();
            connection.Open();

            var sql = @"
                INSERT INTO Campaigns (Id,Name, StartDate, EndDate)
                VALUES (@Id,@Name, @StartDate, @EndDate)";

            await connection.ExecuteAsync(sql, campaign);

            return "Campaign created successfully";
        }

        public async Task<string> CreateCouponAsync(Coupon coupon)
        {
            using var connection = _context.CreateConnection();
            connection.Open();

            // ✅ Validate campaign exists
            var campaign = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT Id FROM Campaigns WHERE Id = @Id",
                new { Id = coupon.CampaignId });

            if (campaign == null)
                throw new Exception("Invalid campaign");

            var sql = @"
                INSERT INTO Coupons 
                (Id,Code, CampaignId, Amount, ExpiryDate, IsRedeemed)
                VALUES (@Id,@Code, @CampaignId, @Amount, @ExpiryDate, 0)";

            await connection.ExecuteAsync(sql, coupon);

            return "Coupon created successfully";
        }

        public async Task<object> ReconcileAsync()
        {
            using var connection = _context.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                //  Find inconsistent records
                var inconsistentTxns = await connection.QueryAsync<dynamic>(
                    @"
                    SELECT t.Id, t.UserId, t.CouponId
                    FROM Transactions t
                    LEFT JOIN Coupons c ON t.CouponId = c.Id
                    WHERE t.Status = 'Success'
                    AND (c.IsRedeemed = 0 OR c.IsRedeemed IS NULL)
                    ",
                    transaction);

                int fixedCount = 0;

                foreach (var txn in inconsistentTxns)
                {
                    // ✅ Fix coupon state
                    await connection.ExecuteAsync(
                        @"
                        UPDATE Coupons
                        SET IsRedeemed = 1,
                            RedeemedBy = @UserId,
                            RedeemedAt = GETUTCDATE()
                        WHERE Id = @CouponId
                        ",
                        new { txn.UserId, txn.CouponId },
                        transaction);

                    fixedCount++;
                }

                transaction.Commit();

                return new
                {
                    message = "Reconciliation completed",
                    fixedRecords = fixedCount
                };
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}