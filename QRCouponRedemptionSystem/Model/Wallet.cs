using System.ComponentModel.DataAnnotations;

namespace QRCouponRedemptionSystem.Model
{
    public class Wallet
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public decimal Balance { get; set; }

        [Timestamp] 
        public byte[] RowVersion { get; set; }
    }
}
