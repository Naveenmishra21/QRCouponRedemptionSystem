using System.ComponentModel.DataAnnotations;

namespace QRCouponRedemptionSystem.Model
{
    public class Wallet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Balance { get; set; }

        [Timestamp] 
        public byte[] RowVersion { get; set; }
    }
}
