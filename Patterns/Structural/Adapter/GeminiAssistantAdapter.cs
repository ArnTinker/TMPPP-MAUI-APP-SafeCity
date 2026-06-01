using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace SafeCity.Patterns.Structural.Adapter;

/// <summary>
/// PATTERN: Adapter (Object Adapter).
/// Justification: The Gemini REST API returns a nested JSON shape that does not match
/// IAssistant. This adapter translates the external contract to the internal one so that
/// ViewModels and tests are completely decoupled from the Gemini-specific response format.
/// Swapping to another LLM (OpenAI, Claude) requires only a new adapter, not editing callers.
/// </summary>
public class GeminiAssistantAdapter : IAssistant
{
    private readonly HttpClient _http;
    private readonly string? _apiKey;

    private const string BaseUrl =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

    public GeminiAssistantAdapter(IHttpClientFactory httpFactory, string? apiKey)
    {
        _http   = httpFactory.CreateClient("Gemini");
        _apiKey = apiKey;
    }

    public bool IsAvailable => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<string> AskAsync(string prompt, CancellationToken ct = default)
    {
        if (!IsAvailable)
            return "SafeCity Assistant is not configured. Add GEMINI_API_KEY to your .env file.";

        try
        {
            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = $"You are SafeCity Assistant, a helpful public-safety advisor. {prompt}" } } }
                }
            };

            var response = await _http.PostAsJsonAsync($"{BaseUrl}?key={_apiKey}", payload, ct);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

            // Adapt Gemini's nested shape → plain string
            return doc.RootElement
                      .GetProperty("candidates")[0]
                      .GetProperty("content")
                      .GetProperty("parts")[0]
                      .GetProperty("text")
                      .GetString() ?? "No response from assistant.";
        }
        catch (Exception ex)
        {
            return $"Assistant error: {ex.Message}";
        }
    }
}
