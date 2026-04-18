namespace QRCouponRedemptionSystem.Model
{
    public class Coupon
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Code { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRedeemed { get; set; }
        public string? RedeemedBy { get; set; }
        public DateTime? RedeemedAt { get; set; }

        public string CampaignId { get; set; }
    }
}
