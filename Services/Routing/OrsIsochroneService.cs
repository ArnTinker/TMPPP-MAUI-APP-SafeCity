using System.Text;
using System.Text.Json;
using SafeCity.Models;

namespace SafeCity.Services.Routing;

/// <summary>
/// ORS implementation of IIsochroneService.
/// Calls POST /v2/isochrones/{profile} and delegates JSON parsing to OrsAdapter.
/// </summary>
public sealed class OrsIsochroneService : IIsochroneService
{
    private readonly HttpClient _http;

    public OrsIsochroneService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("ORS");
    }

    public async Task<IReadOnlyList<GeoCoordinate>> GetReachableAreaAsync(
        GeoCoordinate center,
        int minutes,
        TransportProfile profile = TransportProfile.Walking,
        CancellationToken ct = default)
    {
        var body = JsonSerializer.Serialize(new
        {
            locations  = new[] { new[] { center.Lon, center.Lat } },
            range      = new[] { minutes * 60 },    // ORS expects seconds
            range_type = "time"
        });

        using var request = new HttpRequestMessage(
            HttpMethod.Post, $"/v2/isochrones/{profile.ToOrsProfile()}");
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

        var key = AppConfig.OrsApiKey;
        if (!string.IsNullOrEmpty(key))
            request.Headers.TryAddWithoutValidation("Authorization", key);

        var response = await _http.SendAsync(request, ct);

        if ((int)response.StatusCode == 429)
            throw new RoutingUnavailableException(
                "Routing temporarily unavailable, try again shortly.");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return OrsAdapter.ParseIsochronePolygon(doc.RootElement);
    }
}
