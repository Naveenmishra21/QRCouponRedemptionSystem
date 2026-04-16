using QRCouponRedemptionSystem.Model;

namespace QRCouponRedemptionSystem.Business
{
    public interface IAdminBusiness
    {
        Task<string> CreateCampaignAsync(Campaigns campaign);
        Task<string> CreateCouponAsync(Coupon coupon);
        Task<object> ReconcileAsync();
    }
}
