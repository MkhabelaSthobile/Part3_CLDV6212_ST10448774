using ABC_Retail_App.Models;
using ABCRetailers.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABCRetailers.MVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly IFunctionsApi _api;

        public ProductController(IFunctionsApi api)
        {
            _api = api;
        }

        // GET: /Product
        public async Task<IActionResult> Index(string searchTerm)
        {
            var products = await _api.GetProductsAsync();

            // Apply search/filter by name or description
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products
                    .Where(p => (!string.IsNullOrEmpty(p.ProductName) && p.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // Preserve search/filter values in ViewData
            ViewData["SearchTerm"] = searchTerm;

            return View(products.OrderBy(p => p.ProductName));
        }

        // GET: /Product/Create
        public IActionResult Create() => View();

        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await _api.CreateProductAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: /Product/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var product = await _api.GetProductByIdAsync(id);
            return product == null ? NotFound() : View(product);
        }

        // POST: /Product/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                await _api.UpdateProductAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: /Product/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _api.GetProductByIdAsync(id);
            return product == null ? NotFound() : View(product);
        }

        // POST: /Product/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _api.DeleteProductAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
