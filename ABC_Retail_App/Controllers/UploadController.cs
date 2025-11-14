using ABC_Retail_App.Models;
using ABCRetailers.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ABC_Retail_App.Controllers
{
    public class UploadController : Controller
    {
        private readonly IFunctionsApi _functionsApi;

        public UploadController(IFunctionsApi functionsApi)
        {
            _functionsApi = functionsApi;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "Please select a valid file before uploading.";
                return View();
            }

            try
            {
                // Use the Functions API Client to send the file to the Azure Function
                await _functionsApi.UploadPaymentProofAsync(file);
                ViewBag.Message = "Payment proof uploaded successfully!";
            }
            catch (Exception ex)
            {
                // Log the error (you can replace this with a proper logging mechanism)
                Console.WriteLine($"[Upload Error] {ex.Message}");
                ViewBag.Message = "Failed to upload payment proof. Please try again.";
            }

            return View();
        }
    }
}
