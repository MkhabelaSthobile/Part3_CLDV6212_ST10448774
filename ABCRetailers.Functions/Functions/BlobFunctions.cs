using ABCRetailers.Functions.Helpers;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace ABCRetailers.Functions.Functions
{
    public class BlobFunctions
    {
        private readonly ILogger _logger;

        public BlobFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BlobFunctions>();
        }

        [Function("UploadBlob")]
        public async Task<HttpResponseData> UploadBlob([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UploadBlob")] HttpRequestData req)
        {
            var file = await MultipartHelper.GetFileAsync(req);
            if (file == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("No file uploaded.");
                return badResponse;
            }

            var blobService = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var container = blobService.GetBlobContainerClient("images");
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blob = container.GetBlobClient(file.FileName);
            await blob.UploadAsync(file.Content, overwrite: true);

            _logger.LogInformation($"Blob uploaded: {blob.Uri}");
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { path = blob.Uri.ToString() });
            return response;
        }

        [Function("ProcessUploadedBlob")]
        public void ProcessUploadedBlob([BlobTrigger("images/{name}", Connection = "AzureWebJobsStorage")] BlobClient blobClient, string name)
        {
            _logger.LogInformation($"New blob detected: {name}");
            // Processing logic (e.g., resize image, thumbnail) can go here
        }
    }
}
