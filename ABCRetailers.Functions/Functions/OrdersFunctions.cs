using ABCRetailers.Functions.Entities;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ABCRetailers.Functions.Functions
{
    public class OrdersFunctions
    {
        private readonly ILogger _logger;
        private readonly TableServiceClient _tableServiceClient;
        private readonly QueueClient _queueClient;
        private const string TableName = "Orders";

        public OrdersFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<OrdersFunctions>();
            _tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            _tableServiceClient.CreateTableIfNotExists(TableName);

            _queueClient = new QueueClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "ordersqueue");
            _queueClient.CreateIfNotExists();
        }

        [Function("CreateOrder")]
        public async Task<HttpResponseData> CreateOrder(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateOrder")] HttpRequestData req)
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonSerializer.Deserialize<OrderEntity>(json);
            if (order == null)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid order data.");
                return response;
            }

            order.PartitionKey = "Order";
            order.RowKey = Guid.NewGuid().ToString();

            // Queue the order
            var message = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(order)));
            await _queueClient.SendMessageAsync(message);

            _logger.LogInformation($"Order queued: {order.RowKey}");
            var successResponse = req.CreateResponse(HttpStatusCode.Created);
            await successResponse.WriteStringAsync("Order submitted successfully.");
            return successResponse;
        }

        [Function("GetOrders")]
        public async Task<HttpResponseData> GetOrders(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetOrders")] HttpRequestData req)
        {
            var table = _tableServiceClient.GetTableClient(TableName);
            var orders = table.Query<OrderEntity>().ToList();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(orders));
            return response;
        }

        [Function("UpdateOrderStatus")]
        public async Task<HttpResponseData> UpdateOrderStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "UpdateOrderStatus")] HttpRequestData req)
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonSerializer.Deserialize<OrderEntity>(json);

            if (order == null || string.IsNullOrEmpty(order.RowKey))
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid order data.");
                return response;
            }

            var table = _tableServiceClient.GetTableClient(TableName);
            await table.UpdateEntityAsync(order, ETag.All, TableUpdateMode.Replace);

            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            await successResponse.WriteStringAsync("Order status updated successfully.");
            return successResponse;
        }

        [Function("GetOrderById")]
        public async Task<HttpResponseData> GetOrderById(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders/{id}")] HttpRequestData req,
    string id)
        {
            var table = _tableServiceClient.GetTableClient(TableName);
            try
            {
                var order = await table.GetEntityAsync<OrderEntity>("Order", id);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(JsonSerializer.Serialize(order.Value));
                return response;
            }
            catch
            {
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                await response.WriteStringAsync("Order not found.");
                return response;
            }
        }

    }
}
