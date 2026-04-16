using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCouponRedemptionSystem.Business;
using QRCouponRedemptionSystem.Model;

namespace QRCouponRedemptionSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminBusiness _adminBusiness;

        public AdminController(IAdminBusiness adminBusiness)
        {
            _adminBusiness = adminBusiness;
        }

        [HttpPost("campaign")]
        public async Task<IActionResult> CreateCampaign([FromBody] Campaigns campaign)
        {
            var result = await _adminBusiness.CreateCampaignAsync(campaign);
            return Ok(new { message = result });
        }

        [HttpPost("coupon")]
        public async Task<IActionResult> CreateCoupon([FromBody] Coupon coupon)
        {
            var result = await _adminBusiness.CreateCouponAsync(coupon);
            return Ok(new { message = result });
        }

        [HttpPost("reconcile")]
        public async Task<IActionResult> Reconcile()
        {
            var result = await _adminBusiness.ReconcileAsync();
            return Ok(result);
        }
    }
}
