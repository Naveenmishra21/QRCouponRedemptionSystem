using QRCouponRedemptionSystem.Model;

namespace QRCouponRedemptionSystem.Business
{
    public interface IGlobalUserBusiness
    {
        public Task<Tuple<bool, string>> CheckUserExists(string username, string password);
        public  Task<bool> RegisterUser(User dto);
    }


}
