using ABC_Retail_App.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace ABCRetailers.MVC.Services
{
    public class FunctionsApiClient : IFunctionsApi
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "http://localhost:7071/api"; 

        public FunctionsApiClient(HttpClient httpClient)
        {
            _http = httpClient;
        }

        // Safe JSON read helper (prevents System.Text.Json exceptions)
        private async Task<T?> SafeReadJsonAsync<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                return default;

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                // Optional: log the invalid content here if you want
                return default;
            }
        }

        // ------------------ CUSTOMERS ------------------
        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            var response = await _http.GetAsync($"{BaseUrl}/GetCustomers");
            return await SafeReadJsonAsync<IEnumerable<Customer>>(response) ?? new List<Customer>();
        }

        public async Task<Customer?> GetCustomerByIdAsync(string id)
        {
            var response = await _http.GetAsync($"{BaseUrl}/customers/{id}");
            return await SafeReadJsonAsync<Customer>(response);
        }

        public async Task<bool> CreateCustomerAsync(Customer customer)
        {
            var response = await _http.PostAsJsonAsync($"{BaseUrl}/CreateCustomer", customer);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            var response = await _http.PutAsJsonAsync($"{BaseUrl}/UpdateCustomer", customer);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCustomerAsync(string id)
        {
            var response = await _http.DeleteAsync($"{BaseUrl}/customers/{id}");
            return response.IsSuccessStatusCode;
        }

        // ------------------ PRODUCTS ------------------
        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            var response = await _http.GetAsync($"{BaseUrl}/GetProducts");
            return await SafeReadJsonAsync<IEnumerable<Product>>(response) ?? new List<Product>();
        }

        public async Task<Product?> GetProductByIdAsync(string id)
        {
            var response = await _http.GetAsync($"{BaseUrl}/products/{id}");
            return await SafeReadJsonAsync<Product>(response);
        }

        public async Task<bool> CreateProductAsync(Product product)
        {
            var response = await _http.PostAsJsonAsync($"{BaseUrl}/CreateProduct", product);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            var response = await _http.PutAsJsonAsync($"{BaseUrl}/UpdateProduct", product);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProductAsync(string id)
        {
            var response = await _http.DeleteAsync($"{BaseUrl}/DeleteProduct?id={id}");
            return response.IsSuccessStatusCode;
        }

        // ------------------ ORDERS ------------------
        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            var response = await _http.GetAsync($"{BaseUrl}/GetOrders");
            return await SafeReadJsonAsync<IEnumerable<Order>>(response) ?? new List<Order>();
        }

        public async Task<bool> CreateOrderAsync(Order order)
        {
            var response = await _http.PostAsJsonAsync($"{BaseUrl}/CreateOrder", order);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus)
        {
            var payload = new { Id = orderId, Status = newStatus };
            var response = await _http.PutAsJsonAsync($"{BaseUrl}/UpdateOrderStatus", payload);
            return response.IsSuccessStatusCode;
        }

        public async Task<Order?> GetOrderByIdAsync(string id)
        {
            var response = await _http.GetAsync($"{BaseUrl}/orders/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] {response.StatusCode}: {body}");
                return null;
            }
            return await response.Content.ReadFromJsonAsync<Order>();
        }


        public async Task<bool> DeleteOrderAsync(string id)
        {
            var response = await _http.DeleteAsync($"{BaseUrl}/DeleteOrder?id={id}");
            return response.IsSuccessStatusCode;
        }

        // ------------------ FILE UPLOAD ------------------
        public async Task UploadPaymentProofAsync(IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            var stream = file.OpenReadStream();
            content.Add(new StreamContent(stream), "file", file.FileName);
            await _http.PostAsync($"{BaseUrl}/UploadPaymentProof", content);
        }
    }
}
