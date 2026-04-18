namespace QRCouponRedemptionSystem.Model
{
    public class Transactions
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string CouponId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } 
        public string IdempotencyKey { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
