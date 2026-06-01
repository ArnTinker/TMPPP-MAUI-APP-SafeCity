namespace SafeCity.Patterns.Structural.Proxy;

/// <summary>
/// PATTERN: Proxy (Caching Proxy).
/// Justification: Incident feed cards display media thumbnails. Without a proxy every
/// scroll reloads the image over the network. The proxy intercepts LoadAsync, returns
/// the cached stream on a hit, and delegates to the real loader on a miss — all
/// transparently through the same IMediaLoader interface.
/// </summary>
public class CachedIncidentMediaProxy : IMediaLoader
{
    private readonly IMediaLoader _real;
    private readonly Dictionary<string, byte[]> _cache = new();

    public CachedIncidentMediaProxy(IMediaLoader real) => _real = real;

    public async Task<Stream?> LoadAsync(string url, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(url, out var cached))
            return new MemoryStream(cached);

        var stream = await _real.LoadAsync(url, ct);
        if (stream is null) return null;

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);
        var bytes = ms.ToArray();
        _cache[url] = bytes;
        return new MemoryStream(bytes);
    }

    public void Invalidate(string url) => _cache.Remove(url);
}
