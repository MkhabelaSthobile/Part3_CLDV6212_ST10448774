using ABCRetailers.Functions.Entities;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ABCRetailers.Functions.Functions
{
    public class QueueProcessorFunctions
    {
        private readonly ILogger _logger;
        private readonly TableServiceClient _tableServiceClient;
        private const string TableName = "Orders";

        public QueueProcessorFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<QueueProcessorFunctions>();
            _tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            _tableServiceClient.CreateTableIfNotExists(TableName);
        }

        [Function("ProcessOrderQueue")]
        public async Task ProcessOrderQueue([QueueTrigger("ordersqueue", Connection = "AzureWebJobsStorage")] string queueMessage)
        {
            var order = JsonSerializer.Deserialize<OrderEntity>(Encoding.UTF8.GetString(Convert.FromBase64String(queueMessage)));
            if (order == null) return;

            var table = _tableServiceClient.GetTableClient(TableName);
            await table.AddEntityAsync(order);

            _logger.LogInformation($"Order processed from queue and added to table: {order.RowKey}");
        }
    }
}
