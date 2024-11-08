using GeekShopping.CouponAPI.Data.ValueObjects;
using GeekShopping.CouponAPI.Repository;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CouponAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponController : ControllerBase
    {
        private ICouponRepository _repository;

        public CouponController(ICouponRepository couponRepository)
        {
            _repository = couponRepository;
        }

        [HttpGet("{couponCode}")]

        public async Task<ActionResult<CouponVO>> GetCouponByCouponCode(string couponCode)
        {
            var coupon = await _repository.GetCouponByCouponCode(couponCode);

            if (coupon == null)
                return NotFound();

            return Ok(coupon);
        }
    }
}
