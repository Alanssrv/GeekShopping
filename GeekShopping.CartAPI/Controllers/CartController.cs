using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Repository;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private ICartRepository _repository;

        public CartController(ICartRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpGet("find-cart/{id}")]
        public async Task<ActionResult<CartVO>> FindById(string id)
        {
            var cart = await _repository.FindCartByUserId(id);
            if (cart is null)
                return NotFound();
            return Ok(cart);
        }

        [HttpPost("add-cart")]
        public async Task<ActionResult<CartVO>> AddCart([FromBody] CartVO cartVO)
        {
            var cart = await _repository.SaveOrUpdateCart(cartVO);
            if (cart is null)
                return NotFound();
            return Ok(cart);
        }

        [HttpPut("update-cart")]
        public async Task<ActionResult<CartVO>> UpdateCart(CartVO cartVO)
        {
            var cart = await _repository.SaveOrUpdateCart(cartVO);
            if (cart is null)
                return NotFound();
            return Ok(cart);
        }

        [HttpDelete("remove-cart/{id}")]
        public async Task<ActionResult<bool>> RemoveCart(int id)
        {
            var status = await _repository.RemoveFromCart(id);
            if (!status)
                return BadRequest();
            return Ok(status);
        }

        [HttpPost("apply-coupon")]
        public async Task<ActionResult<bool>> ApplyCoupon([FromBody] CartVO cartVO)
        {
            var status = await _repository.ApplyCoupon(cartVO.CartHeader.UserId, cartVO.CartHeader.CouponCode);
            if (status)
                return Ok(status);
            return NotFound();
        }


        [HttpPost("remove-coupon/{userId}")]
        public async Task<ActionResult<CartVO>> RemoveCoupon(string userId)
        {
            var status = await _repository.RemoveCoupon(userId);
            if (status)
                return Ok(status);
            return NotFound();
        }
    }
}
