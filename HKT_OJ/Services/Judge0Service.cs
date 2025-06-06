using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HKT_OJ.Models;

namespace HKT_OJ.Services
{
    public class Judge0Service
    {
        private readonly HttpClient _httpClient;

        public Judge0Service()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://judge0-ce.p.rapidapi.com/");
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", "bc566068b9msh8d895514b2c30acp1be086jsn1921678b893e");
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", "judge0-ce.p.rapidapi.com");
        }

        public async Task<string> SubmitCodeAsync(SubmissionRequest req)
        {
            string Base64(string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s ?? ""));
            var payload = new
            {
                source_code = Base64(req.SourceCode),
                language_id = req.LanguageId,
                stdin = Base64(req.Input),
                expected_output = Base64(req.ExpectedOutput),
                time_limit = req.TimeLimit,
                memory_limit = req.MemoryLimit
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("submissions?base64_encoded=true&wait=true", content);
            return await response.Content.ReadAsStringAsync();
        }
        public int MapLanguageToId(string language)
        {
            return language.Trim().ToLower() switch
            {
                "cpp" or "c++" => 54,
                "python" => 71,
                "java" => 62,
                "c" => 50,
                "c#" or "csharp" => 51,
                "javascript" => 63,
                _ => throw new Exception($"Unsupported language: {language}")
            };
        }
        public async Task<string> ExecuteAndGetOutputAsync(string sourceCode, string languageId, string input)
        {
            string Base64(string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s ?? ""));

            var payload = new
            {
                source_code = Base64(sourceCode),
                language_id = languageId,
                stdin = Base64(input),
                time_limit = 2.0f,
                memory_limit = 262144
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("submissions?base64_encoded=true&wait=true", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Judge0 response: " + responseBody);

            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.TryGetProperty("stdout", out var stdoutBase64)
                ? Encoding.UTF8.GetString(Convert.FromBase64String(stdoutBase64.GetString() ?? ""))
                : "";
        }

        public async Task<string> RunCodeAndGetOutputWithoutInput(string sourceCode, string languageId)
        {
            string Base64(string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s ?? ""));

            var payload = new
            {
                source_code = Base64(sourceCode),
                language_id = languageId,
                time_limit = 2.0f,
                memory_limit = 262144
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("submissions?base64_encoded=true&wait=true", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Judge0 response2222: " + responseBody);

            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.TryGetProperty("stdout", out var stdoutBase64)
                ? Encoding.UTF8.GetString(Convert.FromBase64String(stdoutBase64.GetString() ?? ""))
                : "";
        }




    }
}
