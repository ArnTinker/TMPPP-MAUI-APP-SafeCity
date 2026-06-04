using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SafeCity.Data;
using SafeCity.Models;
using SafeCity.Patterns.Creational.Singleton;
using SafeCity.Patterns.Structural.Proxy;
using SafeCity.Services.Routing;

namespace SafeCity.ViewModels;

/// <summary>
/// Receives the incident Id via Shell QueryProperty, loads the full record from the
/// repository, increments the view counter, and loads the first photo through the
/// CachedIncidentMediaProxy so caching stays in the display path.
/// </summary>
[QueryProperty(nameof(IncidentId), "id")]
public partial class IncidentDetailViewModel : BaseViewModel
{
    private readonly IIncidentRepository _repo;
    private readonly IMediaLoader        _media;
    private readonly IRoutingService     _routing;
    private readonly SessionManager      _session;

    [ObservableProperty] private int       _incidentId;
    [ObservableProperty] private Incident? _incident;
    [ObservableProperty] private ImageSource? _photoSource;
    [ObservableProperty] private bool      _hasPhoto;
    [ObservableProperty] private string    _routeStatus = string.Empty;

    public IncidentDetailViewModel(
        IIncidentRepository repo,
        IMediaLoader media,
        IRoutingService routing,
        SessionManager session)
    {
        _repo    = repo;
        _media   = media;
        _routing = routing;
        _session = session;
    }

    partial void OnIncidentIdChanged(int value)
    {
        if (value > 0) _ = LoadAsync(value);
    }

    private async Task LoadAsync(int id)
    {
        IsBusy = true;
        HasPhoto = false;
        RouteStatus = string.Empty;
        try
        {
            var inc = await _repo.GetByIdAsync(id);
            if (inc is null) return;

            inc.ViewCount++;
            await _repo.UpdateAsync(inc);
            Incident = inc;

            // Load first photo through CachedIncidentMediaProxy — keeps Proxy in the path
            var path = inc.FirstMediaPath;
            if (path is not null)
            {
                var stream = await _media.LoadAsync(path);
                if (stream is not null)
                {
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    var bytes = ms.ToArray();
                    PhotoSource = ImageSource.FromStream(() => new MemoryStream(bytes));
                    HasPhoto = true;
                }
            }
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RouteHereAsync()
    {
        if (Incident is null) return;
        RouteStatus = "Calculating route…";

        var userLat = _session.LastKnownLat ?? AppConfig.DefaultLat;
        var userLon = _session.LastKnownLon ?? AppConfig.DefaultLon;

        try
        {
            var from  = new GeoCoordinate(userLat, userLon);
            var to    = new GeoCoordinate(Incident.Latitude, Incident.Longitude);
            var route = await _routing.GetRouteAsync(from, to);
            RouteStatus = $"Walking route: {route.Count} waypoints";

            // Open native maps for turn-by-turn navigation
            var location = new Location(Incident.Latitude, Incident.Longitude);
            var options  = new MapLaunchOptions { Name = Incident.Title };
            await Map.Default.OpenAsync(location, options);
        }
        catch (RoutingUnavailableException)
        {
            RouteStatus = "Routing service unavailable — try again later.";
        }
        catch (Exception ex)
        {
            RouteStatus = $"Error: {ex.Message}";
        }
    }
}
