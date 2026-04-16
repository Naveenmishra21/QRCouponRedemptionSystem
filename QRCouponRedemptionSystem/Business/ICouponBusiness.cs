namespace QRCouponRedemptionSystem.Business
{
    public interface ICouponBusiness
    {
        public  Task<object> RedeemAsync(int userId, string couponCode, string idempotencyKey);
        public  Task<decimal> GetWalletBalanceAsync(int userId);
    }
}
