namespace SafeCity.Patterns.Structural.Proxy;

/// <summary>
/// PATTERN: Proxy — Subject interface.
/// Justification: Both the real loader and the caching proxy implement this interface,
/// so callers (IncidentCardView) never know whether they get a cache hit or a network fetch.
/// </summary>
public interface IMediaLoader
{
    Task<Stream?> LoadAsync(string url, CancellationToken ct = default);
    void Invalidate(string url);
}
