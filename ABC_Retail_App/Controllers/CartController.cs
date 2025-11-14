using ABC_Retail_App.Data;
using ABC_Retail_App.Models;
using ABC_Retail_App.Models.ViewModels;
using ABCRetailers.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABC_Retail_App.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<CartController> _logger;

        public CartController(AuthDbContext context, IFunctionsApi functionsApi, ILogger<CartController> logger)
        {
            _context = context;
            _functionsApi = functionsApi;
            _logger = logger;
        }

        // GET: /Cart/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Login");
            }

            var cartViewModel = await GetCartViewModelAsync(username);
            return View(cartViewModel);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(string productId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(productId) || quantity < 1)
            {
                TempData["Error"] = "Invalid product or quantity.";
                return RedirectToAction("Index", "Product");
            }

            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("Login", "Login");
                }

                // Get product details from Azure Functions API
                var product = await _functionsApi.GetProductByIdAsync(productId);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Index", "Product");
                }

                // Check stock availability
                if (product.StockAvailable < quantity)
                {
                    TempData["Error"] = $"Only {product.StockAvailable} items available in stock.";
                    return RedirectToAction("Index", "Product");
                }

                // Check if product already in cart
                var existingCartItem = await _context.Cart
                    .FirstOrDefaultAsync(c => c.CustomerUsername == username && c.ProductId == productId);

                if (existingCartItem != null)
                {
                    // Update quantity
                    existingCartItem.Quantity += quantity;

                    if (existingCartItem.Quantity > product.StockAvailable)
                    {
                        TempData["Error"] = $"Cannot add more items. Only {product.StockAvailable} available.";
                        return RedirectToAction("Index", "Product");
                    }

                    _context.Cart.Update(existingCartItem);
                }
                else
                {
                    // Add new cart item
                    var cartItem = new Cart
                    {
                        CustomerUsername = username,
                        ProductId = productId,
                        Quantity = quantity
                    };
                    _context.Cart.Add(cartItem);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"{product.ProductName} added to cart successfully!";
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product to cart");
                TempData["Error"] = "Failed to add product to cart.";
                return RedirectToAction("Index", "Product");
            }
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            if (quantity < 1)
            {
                TempData["Error"] = "Quantity must be at least 1.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var username = User.Identity?.Name;
                var cartItem = await _context.Cart
                    .FirstOrDefaultAsync(c => c.Id == id && c.CustomerUsername == username);

                if (cartItem == null)
                {
                    TempData["Error"] = "Cart item not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check stock availability
                var product = await _functionsApi.GetProductByIdAsync(cartItem.ProductId);
                if (product != null && product.StockAvailable < quantity)
                {
                    TempData["Error"] = $"Only {product.StockAvailable} items available in stock.";
                    return RedirectToAction(nameof(Index));
                }

                cartItem.Quantity = quantity;
                _context.Cart.Update(cartItem);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cart updated successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart quantity");
                TempData["Error"] = "Failed to update cart.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/RemoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int id)
        {
            try
            {
                var username = User.Identity?.Name;
                var cartItem = await _context.Cart
                    .FirstOrDefaultAsync(c => c.Id == id && c.CustomerUsername == username);

                if (cartItem == null)
                {
                    TempData["Error"] = "Cart item not found.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Cart.Remove(cartItem);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Item removed from cart.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item");
                TempData["Error"] = "Failed to remove item from cart.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/ClearCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var username = User.Identity?.Name;
                var cartItems = await _context.Cart
                    .Where(c => c.CustomerUsername == username)
                    .ToListAsync();

                if (cartItems.Any())
                {
                    _context.Cart.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cart cleared successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                TempData["Error"] = "Failed to clear cart.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/Confirmation - Process checkout directly from cart page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmation()
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("Login", "Login");
                }

                // Get customer information from Azure Functions
                var customers = await _functionsApi.GetCustomersAsync();
                var customer = customers.FirstOrDefault(c => c.Username == username);

                if (customer == null)
                {
                    TempData["Error"] = "Customer profile not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get cart items
                var cartItems = await _context.Cart
                    .Where(c => c.CustomerUsername == username)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction(nameof(Index));
                }

                // Create orders for each cart item
                var orderIds = new List<string>();
                foreach (var item in cartItems)
                {
                    var product = await _functionsApi.GetProductByIdAsync(item.ProductId);
                    if (product == null) continue;

                    var order = new Order
                    {
                        CustomerId = customer.RowKey,
                        Username = username,
                        ProductId = item.ProductId,
                        ProductName = product.ProductName,
                        OrderDate = DateTime.UtcNow,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        TotalPrice = product.Price * item.Quantity,
                        Status = "Submitted"
                    };

                    var success = await _functionsApi.CreateOrderAsync(order);
                    if (success)
                    {
                        orderIds.Add(order.OrderId);
                    }
                }

                // Clear cart after successful checkout
                _context.Cart.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Order placed successfully! {orderIds.Count} order(s) created.";
                TempData["OrderIds"] = string.Join(", ", orderIds.Select(id => id.Substring(0, 8)));

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout");
                TempData["Error"] = "Failed to process checkout. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Cart/GetCartCount - API endpoint for cart count badge
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Json(new { count = 0 });
            }

            var count = await _context.Cart
                .Where(c => c.CustomerUsername == username)
                .SumAsync(c => c.Quantity);

            return Json(new { count });
        }

        // Helper method to build CartViewModel with product details
        private async Task<CartViewModel> GetCartViewModelAsync(string username)
        {
            var cartItems = await _context.Cart
                .Where(c => c.CustomerUsername == username)
                .ToListAsync();

            var cartItemViewModels = new List<CartItemViewModel>();

            foreach (var item in cartItems)
            {
                var product = await _functionsApi.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    cartItemViewModels.Add(new CartItemViewModel
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = product.ProductName,
                        ProductImageUrl = product.ImageUrl,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        StockAvailable = product.StockAvailable
                    });
                }
            }

            return new CartViewModel
            {
                CustomerUsername = username,
                CartItems = cartItemViewModels
            };
        }
    }
}