using ABC_Retail_App.Models;
using ABC_Retail_App.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using ABCRetailers.MVC.Services;

namespace ABC_Retail_App.Controllers
{
    public class OrderController : Controller
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IFunctionsApi functionsApi, ILogger<OrderController> logger)
        {
            _functionsApi = functionsApi;
            _logger = logger;
        }

        // GET: /Order
        public async Task<IActionResult> Index()
        {
            var orders = await _functionsApi.GetOrdersAsync();
            return View(orders);
        }

        // GET: /Order/Create
        public async Task<IActionResult> Create()
        {
            var customers = await _functionsApi.GetCustomersAsync();
            var products = await _functionsApi.GetProductsAsync();

            var viewModel = new OrderCreateViewModel
            {
                Customers = (List<Customer>)customers,
                Products = (List<Product>)products
            };

            return View(viewModel);
        }

        // POST: /Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var customer = await _functionsApi.GetCustomerByIdAsync(model.CustomerId);
                var product = await _functionsApi.GetProductByIdAsync(model.ProductId);

                if (customer == null || product == null)
                {
                    ModelState.AddModelError("", "Invalid customer or product selected.");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                if (product.StockAvailable < model.Quantity)
                {
                    ModelState.AddModelError("Quantity", $"Insufficient stock. Available: {product.StockAvailable}");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                var order = new Order
                {
                    CustomerId = model.CustomerId,
                    Username = customer.Username,
                    ProductId = model.ProductId,
                    ProductName = product.ProductName,
                    OrderDate = DateTime.UtcNow,
                    Quantity = model.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * model.Quantity,
                    Status = "Submitted"
                };

                var success = await _functionsApi.CreateOrderAsync(order);

                if (success)
                {
                    TempData["Success"] = "Order created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Failed to create order. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                ModelState.AddModelError("", $"Error creating order: {ex.Message}");
            }

            await PopulateDropdowns(model);
            return View(model);
        }

        // GET: /Order/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            var order = await _functionsApi.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        // GET: /Order/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var order = await _functionsApi.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: /Order/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Order order)
        {
            if (!ModelState.IsValid)
                return View(order);

            var success = await _functionsApi.UpdateOrderStatusAsync(order.OrderId, order.Status);
            if (success)
            {
                TempData["Success"] = "Order updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to update order.");
            return View(order);
        }

        // DELETE
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _functionsApi.DeleteOrderAsync(id);
            if (success)
                TempData["Success"] = "Order deleted successfully!";
            else
                TempData["Error"] = "Failed to delete order.";

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(OrderCreateViewModel model)
        {
            model.Customers = (List<Customer>)await _functionsApi.GetCustomersAsync();
            model.Products = (List<Product>)await _functionsApi.GetProductsAsync();
        }
    }
}
