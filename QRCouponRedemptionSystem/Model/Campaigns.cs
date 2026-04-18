namespace QRCouponRedemptionSystem.Model
{
    public class Campaigns
    {
        public string Id { get; set; } = Guid.NewGuid.ToString();
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
