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
    public class UploadsFunctions
    {
        private readonly ILogger _logger;

        public UploadsFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UploadsFunctions>();
        }

        [Function("UploadPaymentProof")]
        public async Task<HttpResponseData> UploadPaymentProof([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UploadPaymentProof")] HttpRequestData req)
        {
            var file = await MultipartHelper.GetFileAsync(req);
            if (file == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("No file uploaded.");
                return badResponse;
            }

            try
            {
                var blobService = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
                var container = blobService.GetBlobContainerClient("uploads");
                await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blobClient = container.GetBlobClient(file.FileName);
                await blobClient.UploadAsync(file.Content, overwrite: true);

                _logger.LogInformation($"File uploaded: {blobClient.Uri}");
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new { path = blobClient.Uri.ToString() });
                return response;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error uploading file: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error uploading file.");
                return errorResponse;
            }
        }
    }
}
