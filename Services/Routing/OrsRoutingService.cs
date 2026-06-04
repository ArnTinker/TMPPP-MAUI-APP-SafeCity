using System.Text;
using System.Text.Json;
using SafeCity.Models;

namespace SafeCity.Services.Routing;

/// <summary>
/// ORS implementation of IRoutingService.
/// Calls POST /v2/directions/{profile}/geojson and delegates JSON parsing to OrsAdapter.
/// </summary>
public sealed class OrsRoutingService : IRoutingService
{
    private readonly HttpClient _http;

    public OrsRoutingService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("ORS");
    }

    public async Task<IReadOnlyList<GeoCoordinate>> GetRouteAsync(
        GeoCoordinate from,
        GeoCoordinate to,
        TransportProfile profile = TransportProfile.Walking,
        CancellationToken ct = default)
    {
        var body = JsonSerializer.Serialize(new
        {
            coordinates = new[] { new[] { from.Lon, from.Lat }, new[] { to.Lon, to.Lat } },
            instructions = false
        });

        using var request = new HttpRequestMessage(
            HttpMethod.Post, $"/v2/directions/{profile.ToOrsProfile()}/geojson");
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        AddAuth(request);

        var response = await _http.SendAsync(request, ct);
        EnsureNotRateLimited(response);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return OrsAdapter.ParseRouteCoordinates(doc.RootElement);
    }

    private static void AddAuth(HttpRequestMessage req)
    {
        var key = AppConfig.OrsApiKey;
        if (!string.IsNullOrEmpty(key))
            req.Headers.TryAddWithoutValidation("Authorization", key);
    }

    private static void EnsureNotRateLimited(HttpResponseMessage response)
    {
        if ((int)response.StatusCode == 429)
            throw new RoutingUnavailableException(
                "Routing temporarily unavailable, try again shortly.");
    }
}
