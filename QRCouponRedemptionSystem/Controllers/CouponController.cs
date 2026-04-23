using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCouponRedemptionSystem.Business;
using QRCouponRedemptionSystem.Model;
using System.Security.Claims;
using System.Transactions;

namespace QRCouponRedemptionSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponController : ControllerBase
    {
        private readonly ICouponBusiness _couponService;

        public CouponController(ICouponBusiness couponService)
        {
            _couponService = couponService;
        }
        [HttpPost("redeem")]
        public async Task<IActionResult> Redeem(string CouponCode,  string userId)
        {
            if (string.IsNullOrEmpty(CouponCode))
                return BadRequest("CouponCode required");
            if(string.IsNullOrEmpty(userId))
                return BadRequest("UserId required");
            //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            try
            {
                string idempotencyKey = Guid.NewGuid().ToString(); 
                var result = await _couponService.RedeemAsync(userId, CouponCode, idempotencyKey);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("wallet")]
        public async Task<IActionResult> GetWalletBalance(string userId)
        {
            try
            {
                //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var balance = await _couponService.GetWalletBalanceAsync(userId);

                return Ok(new
                {
                    balance = balance
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }




    }
}
