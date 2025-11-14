using ABC_Retail_App.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABCRetailers.MVC.Services
{
    public interface IFunctionsApi
    {
        // Customers
        Task<IEnumerable<Customer>> GetCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(string id);
        Task<bool> CreateCustomerAsync(Customer customer);
        Task<bool> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(string id);

        // Products
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<Product?> GetProductByIdAsync(string id);
        Task<bool> CreateProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(string id);

        // Orders
        Task<IEnumerable<Order>> GetOrdersAsync();
        Task<Order?> GetOrderByIdAsync(string id);

        Task<bool> CreateOrderAsync(Order order);
        Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus);

        // Uploads
        Task UploadPaymentProofAsync(IFormFile file);
        //Task<string?> GetOrderByIdAsync(string id);
        Task<bool> DeleteOrderAsync(string id);
    }
}
