namespace QRCouponRedemptionSystem.Model
{
    public class Transactions
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CouponId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } 
        public string IdempotencyKey { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
