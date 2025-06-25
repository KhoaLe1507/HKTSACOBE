using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace HKT_OJ.Services
{

    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["Gemini:ApiKey"]; // đọc từ appsettings
        }

        public async Task<string> AskGeminiAsync(string prompt, string model = "gemini-2.0-flash")
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                role = "user",
                parts = new[] { new { text = prompt } }
            }
        }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("candidates")[0]
                      .GetProperty("content")
                      .GetProperty("parts")[0]
                      .GetProperty("text")
                      .GetString() ?? "No response from Gemini.";
        }

    }

}
