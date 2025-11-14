using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker.Http;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ABCRetailers.Functions.Helpers
{
    public static class MultipartHelper
    {
        public class FileData
        {
            public string FileName { get; set; }
            public Stream Content { get; set; }
        }

        public static async Task<FileData> GetFileAsync(HttpRequestData req)
        {
            if (!req.Headers.TryGetValues("Content-Type", out var contentType) ||
                !contentType.ToString().Contains("multipart/form-data"))
                return null;

            var boundary = contentType.ToString().Split("boundary=")[1];
            var stream = new MemoryStream();
            await req.Body.CopyToAsync(stream);
            stream.Position = 0;

            var reader = new MultipartReader(boundary, stream);
            var section = await reader.ReadNextSectionAsync();
            if (section == null) return null;

            var fileName = ContentDispositionHeaderValue.Parse(section.ContentDisposition).FileName.Trim('"');
            var fileStream = new MemoryStream();
            await section.Body.CopyToAsync(fileStream);
            fileStream.Position = 0;

            return new FileData { FileName = fileName, Content = fileStream };
        }
    }
}
