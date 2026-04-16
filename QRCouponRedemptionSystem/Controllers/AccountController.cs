using Microsoft.AspNetCore.Mvc;
using QRCouponRedemptionSystem.Business;
using QRCouponRedemptionSystem.Model;

namespace QRCouponRedemptionSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly IGlobalUserBusiness _globalUserBusiness;

        public AccountController(JwtService jwtService, IGlobalUserBusiness globalUserBusiness)
        {
            _jwtService = jwtService;
            _globalUserBusiness = globalUserBusiness;
        }

        [HttpPost("Login")]
        public IActionResult Login(string username, string password)
        {
            var isValidUser = _globalUserBusiness.CheckUserExists(username, password).Result;
            if (isValidUser.Item1)
            {
                var token = _jwtService.GenerateToken(isValidUser.Item2);
                return Ok(new{ isValidUser.Item2 });
            }
            return Unauthorized();
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

            var result = await _globalUserBusiness.RegisterUser(dto);

            if (!result)
                return BadRequest("User already exists");

            return Ok("User registered successfully");
        }





    }
}
