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
            // Local file path — read directly so the proxy can still cache the bytes
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                if (!File.Exists(url)) return null;
                var local = await File.ReadAllBytesAsync(url, ct);
                return new MemoryStream(local);
            }

            var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            return new MemoryStream(bytes);
        }
        catch { return null; }
    }

    public void Invalidate(string url) { /* no-op for real loader */ }
}
