using System.Net.Http;

namespace SafeCity.Patterns.Structural.Proxy;

/// <summary>Real subject — performs the actual HTTP download.</summary>
public class RealMediaLoader : IMediaLoader
{
    private readonly HttpClient _http;

    public RealMediaLoader(IHttpClientFactory factory) =>
        _http = factory.CreateClient("Media");

    public async Task<Stream?> LoadAsync(string url, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            return new MemoryStream(bytes);
        }
        catch { return null; }
    }

    public void Invalidate(string url) { /* no-op for real loader */ }
}
