using ABCRetailers.Functions.Entities;
using ABCRetailers.Functions.Helpers;
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
    public class ProductsFunctions
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly ILogger _logger;
        private const string TableName = "Products";

        public ProductsFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProductsFunctions>();
            _tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            _tableServiceClient.CreateTableIfNotExists(TableName);
        }

        [Function("CreateProduct")]
        public async Task<HttpResponseData> CreateProduct([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateProduct")] HttpRequestData req)
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            var product = JsonSerializer.Deserialize<ProductEntity>(json);

            if (product == null)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid product data.");
                return response;
            }

            product.RowKey = Guid.NewGuid().ToString();
            var tableClient = _tableServiceClient.GetTableClient(TableName);
            await tableClient.AddEntityAsync(product);

            _logger.LogInformation($"Product added: {product.ProductName}");
            var successResponse = req.CreateResponse(HttpStatusCode.Created);
            await successResponse.WriteStringAsync("Product successfully created.");
            return successResponse;
        }

        [Function("GetProducts")]
        public async Task<HttpResponseData> GetProducts([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetProducts")] HttpRequestData req)
        {
            var tableClient = _tableServiceClient.GetTableClient(TableName);
            var entities = tableClient.Query<ProductEntity>().ToList();
            var dtos = entities.Select(Map.ToDto).ToList();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(dtos));
            return response;
        }

        [Function("UpdateProduct")]
        public async Task<HttpResponseData> UpdateProduct([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "UpdateProduct")] HttpRequestData req)
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedProduct = JsonSerializer.Deserialize<ProductEntity>(json);

            if (updatedProduct == null || string.IsNullOrEmpty(updatedProduct.RowKey))
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid product data.");
                return response;
            }

            var tableClient = _tableServiceClient.GetTableClient(TableName);
            await tableClient.UpdateEntityAsync(updatedProduct, ETag.All, TableUpdateMode.Replace);

            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            await successResponse.WriteStringAsync("Product updated successfully.");
            return successResponse;
        }

        [Function("DeleteProduct")]
        public async Task<HttpResponseData> DeleteProduct([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "DeleteProduct")] HttpRequestData req)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var rowKey = query["id"];

            if (string.IsNullOrEmpty(rowKey))
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Product ID is required.");
                return response;
            }

            var tableClient = _tableServiceClient.GetTableClient(TableName);
            await tableClient.DeleteEntityAsync("Product", rowKey);

            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            await successResponse.WriteStringAsync("Product deleted successfully.");
            return successResponse;
        }
    }
}
