using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SchoolInfo.Infrastructure.AI;

public class SchoolAIAgent
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _instructions;

    public SchoolAIAgent(HttpClient httpClient, string apiKey, string model, string instructions)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _model = model;
        _instructions = instructions;
    }

    public async Task<string> RunAsync(string input)
    {
        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_API_KEY")
        {
            return "Bugün okulda her şey harikaydı. Yemeklerini güzelce yedi.";
        }

        // Gemini API URL
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

        // Build the request payload according to the Gemini API spec
        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = input }
                    }
                }
            },
            systemInstruction = new
            {
                parts = new[]
                {
                    new { text = _instructions }
                }
            },
            generationConfig = new
            {
                temperature = 0.2
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"Gemini API error ({response.StatusCode}): {err}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text ?? "Bugün okulda her şey harikaydı. Yemeklerini güzelce yedi.";
    }
}
