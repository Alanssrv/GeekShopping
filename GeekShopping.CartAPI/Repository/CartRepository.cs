using AutoMapper;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Model;
using GeekShopping.CartAPI.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CartAPI.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly SQLContext _context;
        private IMapper _mapper;

        public CartRepository(SQLContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> ApplyCoupon(string userId, string couponCode)
        {
            var header = await _context.CartHeaders.FirstOrDefaultAsync(
                c => c.UserId == userId
            );

            if (header is not null)
            {
                header.CouponCode = couponCode;

                _context.CartHeaders.Update(header);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> ClearCart(string userId)
        {
            var cartHeader = await _context.CartHeaders.FirstOrDefaultAsync(
                c => c.UserId == userId
            );

            if (cartHeader is not null)
            {
                _context.CartDetails.RemoveRange(
                    _context.CartDetails.Where(c => c.CartHeaderId == cartHeader.Id)
                );
                _context.CartHeaders.Remove(cartHeader);

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<CartVO> FindCartByUserId(string userId)
        {
            Cart cart = new Cart()
            {
                CartHeader = await _context.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId),
            };

            cart.CartDetails = _context.CartDetails
                .Where(c => c.CartHeaderId == cart.CartHeader.Id)
                .Include(c => c.Product);

            return _mapper.Map<CartVO>(cart);
        }

        public async Task<bool> RemoveCoupon(string userId)
        {
            var header = await _context.CartHeaders.FirstOrDefaultAsync(
                c => c.UserId == userId
            );

            if (header is not null)
            {
                header.CouponCode = null;

                _context.CartHeaders.Update(header);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> RemoveFromCart(long cartDetailsId)
        {
            try
            {
                CartDetail cartDetail = await _context.CartDetails.FirstOrDefaultAsync(
                    c => c.Id == cartDetailsId
                );

                int total = _context.CartDetails
                    .Where(c => c.CartHeaderId == cartDetail.CartHeaderId)
                    .Count();

                _context.CartDetails.Remove(cartDetail);
                if (total == 1)
                {
                    var cartHeaderToRemove = await _context.CartHeaders.FirstOrDefaultAsync(
                        c => c.Id == cartDetail.CartHeaderId
                    );
                    _context.CartHeaders.Remove(cartHeaderToRemove);
                }

                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<CartVO> SaveOrUpdateCart(CartVO cartVO)
        {
            Cart cart = _mapper.Map<Cart>(cartVO);

            #region Check and save new Product
            var product = await _context.Products.FirstOrDefaultAsync(
                p => p.Id == cartVO.CartDetails.FirstOrDefault().ProductId
            );

            if (product is null)
            {
                _context.Products.Add(cart.CartDetails.FirstOrDefault().Product);
                await _context.SaveChangesAsync();
            }
            #endregion

            #region Check and save new CartHeader
            var cartHeader = await _context.CartHeaders.AsNoTracking().FirstOrDefaultAsync(
                c => c.UserId == cart.CartHeader.UserId
            );

            if (cartHeader is null)
            {
                var cartDetail = cart.CartDetails.FirstOrDefault();

                _context.CartHeaders.Add(cart.CartHeader);
                await _context.SaveChangesAsync();
                cartDetail.CartHeaderId = cart.CartHeader.Id;
                cartDetail.Product = null;

                _context.CartDetails.Add(cartDetail);
                await _context.SaveChangesAsync();
            }
            else
            {
                var cartDetail = await _context.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                    p => p.ProductId == cart.CartDetails.FirstOrDefault().ProductId && p.CartHeaderId == cartHeader.Id
                );

                if (cartDetail is null)
                {
                    var newCartDetail = cart.CartDetails.FirstOrDefault();

                    newCartDetail.CartHeaderId = cartHeader.Id;
                    newCartDetail.Product = null;

                    _context.CartDetails.Add(newCartDetail);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var updateCartDetail = cart.CartDetails.FirstOrDefault();

                    updateCartDetail.Product = null;
                    updateCartDetail.Count += cartDetail.Count;
                    updateCartDetail.Id = cartDetail.Id;
                    updateCartDetail.CartHeaderId = cartDetail.CartHeaderId;

                    _context.CartDetails.Update(updateCartDetail);

                    await _context.SaveChangesAsync();
                }
            }
            #endregion

            return _mapper.Map<CartVO>(cart);
        }
    }
}
