using ABC_Retail_App.Models;
using ABCRetailers.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ABCRetailers.MVC.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IFunctionsApi _api;

        public CustomerController(IFunctionsApi api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index() => View(await _api.GetCustomersAsync());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _api.CreateCustomerAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var customer = await _api.GetCustomerByIdAsync(id);
            return customer == null ? NotFound() : View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _api.UpdateCustomerAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var customer = await _api.GetCustomerByIdAsync(id);
            return customer == null ? NotFound() : View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _api.DeleteCustomerAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
