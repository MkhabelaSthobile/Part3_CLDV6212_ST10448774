using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;

namespace ABCRetailers.Functions.Helpers
{
    public static class HttpJson
    {
        public static async Task<T> ReadJsonAsync<T>(HttpRequestData req)
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            return JsonSerializer.Deserialize<T>(json);
        }

        public static async Task WriteJsonAsync<T>(HttpResponseData res, T data)
        {
            res.Headers.Add("Content-Type", "application/json");
            await res.WriteStringAsync(JsonSerializer.Serialize(data));
        }
    }

}
