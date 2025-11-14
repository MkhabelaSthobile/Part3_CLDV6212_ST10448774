using System.Diagnostics;
using ABC_Retail_App.Models;
using ABC_Retail_App.Models.ViewModels;
using ABCRetailers.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace ABC_Retail_App.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFunctionsApi _functionsApi;

        public HomeController(IFunctionsApi functionsApi)
        {
            _functionsApi = functionsApi;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get data from the Azure Functions API
                var products = await _functionsApi.GetProductsAsync();
                var customers = await _functionsApi.GetCustomersAsync();
                var orders = await _functionsApi.GetOrdersAsync();

                var viewModel = new HomeViewModel
                {
                    FeaturedProducts = products.Take(5).ToList(),
                    ProductCount = products.Count(),
                    CustomerCount = customers.Count(),
                    OrderCount = orders.Count()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading data: {ex.Message}";
                return View(new HomeViewModel());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> InitializeStorage()
        {
            try
            {
                // Use any simple API call to confirm connection works
                await _functionsApi.GetCustomersAsync();
                TempData["Success"] = "Azure Functions API connected successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to connect to Azure Functions API: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
