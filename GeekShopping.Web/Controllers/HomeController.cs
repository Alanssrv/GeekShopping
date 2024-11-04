using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GeekShopping.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public HomeController(ILogger<HomeController> logger, IProductService productService, ICartService cartService)
        {
            _logger = logger;
            _productService = productService;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.FindAllProducts(string.Empty);
            return View(products);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var product = await _productService.FindProductById(id, accessToken);

            return View(product);
        }

        [HttpPost]
        [ActionName("Details")]
        [Authorize]
        public async Task<IActionResult> DetailsPost(ProductViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            CartViewModel cart = new CartViewModel()
            {
                CartHeader = new CartHeaderViewModel()
                {
                    UserId = User.Claims.Where(user => user.Type == "sub")?.FirstOrDefault()?.Value,

                },
                CartDetails = new List<CartDetailViewModel>()
                {
                    new CartDetailViewModel()
                    {
                        Count = model.Count,
                        ProductId = model.Id,
                        Product = await _productService.FindProductById(model.Id, accessToken),
                    }
                }
            };

            var response = await _cartService.AddItemToCart(cart, accessToken);

            if (response is not null)
                return RedirectToAction(nameof(Index));

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> Login()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }
    }
}
