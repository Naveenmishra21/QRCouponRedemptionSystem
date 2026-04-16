namespace QRCouponRedemptionSystem.Model
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRedeemed { get; set; }
        public int? RedeemedBy { get; set; }
        public DateTime? RedeemedAt { get; set; }

        public int CampaignId { get; set; }
    }
}
