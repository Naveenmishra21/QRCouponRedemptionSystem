namespace QRCouponRedemptionSystem.Business
{
    public interface ICouponBusiness
    {
        public  Task<object> RedeemAsync(string userId, string couponCode, string idempotencyKey);
        public  Task<decimal> GetWalletBalanceAsync(string userId);
    }
}
