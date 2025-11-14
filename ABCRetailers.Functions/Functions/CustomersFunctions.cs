using ABCRetailers.Functions.Entities;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace ABCRetailers.Functions.Functions
{
    public class CustomersFunctions
    {
        private readonly ILogger _logger;
        private readonly TableServiceClient _tableServiceClient;
        private const string TableName = "Customers";

        public CustomersFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CustomersFunctions>();
            _tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            _tableServiceClient.CreateTableIfNotExists(TableName);
        }

        [Function("CreateCustomer")]
        public async Task<HttpResponseData> CreateCustomer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateCustomer")] HttpRequestData req)
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            var customer = JsonSerializer.Deserialize<CustomerEntity>(json);

            if (customer == null)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid customer data.");
                return response;
            }

            customer.PartitionKey = "Customer";
            customer.RowKey = Guid.NewGuid().ToString();

            var table = _tableServiceClient.GetTableClient(TableName);
            await table.AddEntityAsync(customer);

            _logger.LogInformation($"Customer created: {customer.Name}");
            var successResponse = req.CreateResponse(HttpStatusCode.Created);
            await successResponse.WriteStringAsync("Customer successfully created.");
            return successResponse;
        }

        [Function("GetCustomers")]
        public async Task<HttpResponseData> GetCustomers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetCustomers")] HttpRequestData req)
        {
            var table = _tableServiceClient.GetTableClient(TableName);
            var customers = table.Query<CustomerEntity>().ToList();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(customers));
            return response;
        }

        [Function("GetCustomerById")]
        public async Task<HttpResponseData> GetCustomerById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{id}")] HttpRequestData req,
            string id)
        {
            var table = _tableServiceClient.GetTableClient(TableName);
            try
            {
                var customer = await table.GetEntityAsync<CustomerEntity>("Customer", id);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(JsonSerializer.Serialize(customer.Value));
                return response;
            }
            catch
            {
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                await response.WriteStringAsync("Customer not found.");
                return response;
            }
        }

        [Function("UpdateCustomer")]
        public async Task<HttpResponseData> UpdateCustomer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "UpdateCustomer")] HttpRequestData req)
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            var customer = JsonSerializer.Deserialize<CustomerEntity>(json);

            if (customer == null || string.IsNullOrEmpty(customer.RowKey))
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid customer data.");
                return response;
            }

            var table = _tableServiceClient.GetTableClient(TableName);
            await table.UpdateEntityAsync(customer, ETag.All, TableUpdateMode.Replace);

            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            await successResponse.WriteStringAsync("Customer updated successfully.");
            return successResponse;
        }

        [Function("DeleteCustomer")]
        public async Task<HttpResponseData> DeleteCustomer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "customers/{id}")] HttpRequestData req,
            string id)
        {
            var table = _tableServiceClient.GetTableClient(TableName);
            await table.DeleteEntityAsync("Customer", id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Customer deleted successfully.");
            return response;
        }
    }
}
