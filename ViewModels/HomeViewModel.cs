using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SafeCity.Data;
using SafeCity.Models;
using SafeCity.Patterns.Behavioral.Observer;
using SafeCity.Patterns.Creational.FactoryMethod;
using SafeCity.Patterns.Creational.Singleton;
using SafeCity.Patterns.Structural.Adapter;
using SafeCity.Services;
using SafeCity.Services.Routing;

namespace SafeCity.ViewModels;

public partial class HomeViewModel : BaseViewModel, IIncidentObserver
{
    private readonly IIncidentRepository     _repo;
    private readonly IIncidentCreator        _factory;
    private readonly SessionManager          _session;
    private readonly ILocationService        _location;
    private readonly IncidentAlertPublisher  _publisher;
    private readonly IMapProvider            _map;
    private readonly IRoutingService         _routing;
    private readonly IGeocodingService       _geocoding;
    private readonly IIsochroneService       _isochrone;

    [ObservableProperty] private ObservableCollection<Incident>       _nearbyIncidents  = [];
    [ObservableProperty] private ObservableCollection<GeocodingResult> _searchResults   = [];
    [ObservableProperty] private double  _userLat          = AppConfig.DefaultLat;
    [ObservableProperty] private double  _userLon          = AppConfig.DefaultLon;
    [ObservableProperty] private int     _nearbyCount;
    [ObservableProperty] private string  _nearbyUsers      = "—";
    [ObservableProperty] private string  _searchText       = "";
    [ObservableProperty] private bool    _showSearchResults;
    [ObservableProperty] private string  _routeStatusMessage = "";
    [ObservableProperty] private bool    _hasActiveRoute;
    [ObservableProperty] private GeocodingResult? _selectedSearchResult;

    private CancellationTokenSource? _searchCts;
    private CancellationTokenSource? _routeCts;

    public HomeViewModel(
        IIncidentRepository repo, IIncidentCreator factory,
        SessionManager session, ILocationService location,
        IncidentAlertPublisher publisher, IMapProvider map,
        IRoutingService routing, IGeocodingService geocoding, IIsochroneService isochrone)
    {
        _repo      = repo;
        _factory   = factory;
        _session   = session;
        _location  = location;
        _publisher = publisher;
        _map       = map;
        _routing   = routing;
        _geocoding = geocoding;
        _isochrone = isochrone;

        _publisher.Subscribe(this);
        _map.MarkerTapped += OnMarkerTapped;
    }

    // ── Load ───────────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;

        var loc = await _location.GetCurrentLocationAsync();
        if (loc.HasCoords)
        {
            UserLat = loc.Lat;
            UserLon = loc.Lon;
            _session.UpdateLocation(UserLat, UserLon);
        }

        var nearby = await _repo.GetNearbyAsync(UserLat, UserLon, AppConfig.NearbyRadiusKm);
        NearbyIncidents = new ObservableCollection<Incident>(nearby.Select(_factory.Hydrate));
        NearbyCount = NearbyIncidents.Count;

        _map.CenterOn(UserLat, UserLon);
        _map.SetUserLocation(UserLat, UserLon);
        _map.ClearMarkers();
        foreach (var incident in NearbyIncidents)
            _map.PlaceMarker(incident);

        IsBusy = false;
    }

    [RelayCommand]
    private async Task GoToReportAsync() =>
        await Shell.Current.GoToAsync("//ReportPage");

    // ── Search (address geocoding with 400ms debounce) ─────────────────────────

    partial void OnSearchTextChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = new CancellationTokenSource();
        _ = DebounceSearchAsync(value, _searchCts.Token);
    }

    private async Task DebounceSearchAsync(string text, CancellationToken ct)
    {
        try
        {
            await Task.Delay(400, ct);

            if (string.IsNullOrWhiteSpace(text))
            {
                SearchResults = [];
                ShowSearchResults = false;
                return;
            }

            var focus = new GeoCoordinate(UserLat, UserLon);
            var results = await _geocoding.SearchAsync(text, focus, ct);
            SearchResults = new ObservableCollection<GeocodingResult>(results.Take(5));
            ShowSearchResults = SearchResults.Count > 0;
        }
        catch (OperationCanceledException) { }
        catch (RoutingUnavailableException ex) { RouteStatusMessage = ex.Message; }
        catch { /* silent — user can retry */ }
    }

    // Called when user taps a geocoding result row
    partial void OnSelectedSearchResultChanged(GeocodingResult? value)
    {
        if (value is null) return;
        _map.CenterOn(value.Coordinate.Lat, value.Coordinate.Lon, 15);
        ShowSearchResults = false;
        SearchText = value.Label;
        SelectedSearchResult = null; // allow re-selecting same item
    }

    [RelayCommand]
    private void DismissSearch()
    {
        ShowSearchResults = false;
        SearchResults = [];
    }

    // ── Route ─────────────────────────────────────────────────────────────────

    [RelayCommand]
    private void ClearRoute()
    {
        _routeCts?.Cancel();
        _map.ClearRoute();
        HasActiveRoute      = false;
        RouteStatusMessage  = "";
    }

    private async Task RouteToAsync(GeoCoordinate destination, TransportProfile profile)
    {
        _routeCts?.Cancel();
        _routeCts?.Dispose();
        _routeCts = new CancellationTokenSource();

        RouteStatusMessage = "Routing…";
        HasActiveRoute     = false;

        try
        {
            var from   = new GeoCoordinate(UserLat, UserLon);
            var coords = await _routing.GetRouteAsync(from, destination, profile, _routeCts.Token);

            if (coords.Count == 0)
            {
                RouteStatusMessage = "No route found for this location.";
                return;
            }

            _map.DrawRoute(coords);
            HasActiveRoute     = true;
            RouteStatusMessage = $"Route ready · {profile.ToDisplayName()}";
        }
        catch (RoutingUnavailableException ex) { RouteStatusMessage = ex.Message; }
        catch (OperationCanceledException)    { RouteStatusMessage = ""; }
        catch                                 { RouteStatusMessage = "Could not fetch route. Check your connection."; }
    }

    private async Task ShowAlertZoneAsync(GeoCoordinate center)
    {
        _routeCts?.Cancel();
        _routeCts?.Dispose();
        _routeCts = new CancellationTokenSource();

        RouteStatusMessage = "Calculating alert zone…";
        HasActiveRoute     = false;

        try
        {
            var polygon = await _isochrone.GetReachableAreaAsync(
                center, minutes: 5, TransportProfile.Walking, _routeCts.Token);

            if (polygon.Count == 0)
            {
                RouteStatusMessage = "Could not calculate alert zone.";
                return;
            }

            _map.DrawIsochrone(polygon);
            HasActiveRoute     = true;
            RouteStatusMessage = "Alert zone · 5 min walk radius";
        }
        catch (RoutingUnavailableException ex) { RouteStatusMessage = ex.Message; }
        catch (OperationCanceledException)    { RouteStatusMessage = ""; }
        catch                                 { RouteStatusMessage = "Could not calculate alert zone."; }
    }

    // ── IIncidentObserver ──────────────────────────────────────────────────────

    public void OnIncidentPublished(Incident incident)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var hydrated = _factory.Hydrate(incident);
            NearbyIncidents.Insert(0, hydrated);
            NearbyCount = NearbyIncidents.Count;
            _map.PlaceMarker(hydrated);
        });
    }

    // ── Map interaction ────────────────────────────────────────────────────────

    private void OnMarkerTapped(int incidentId)
    {
        var incident = NearbyIncidents.FirstOrDefault(i => i.Id == incidentId);
        if (incident is null) return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var action = await Shell.Current.DisplayActionSheetAsync(
                incident.Title, "Cancel", null,
                "🚶 Route here (Walk)",
                "🚗 Route here (Drive)",
                "🚲 Route here (Cycle)",
                "📡 Alert Zone (5 min)");

            var dest = new GeoCoordinate(incident.Latitude, incident.Longitude);

            switch (action)
            {
                case "🚶 Route here (Walk)":  await RouteToAsync(dest, TransportProfile.Walking);  break;
                case "🚗 Route here (Drive)": await RouteToAsync(dest, TransportProfile.Driving);  break;
                case "🚲 Route here (Cycle)": await RouteToAsync(dest, TransportProfile.Cycling);  break;
                case "📡 Alert Zone (5 min)": await ShowAlertZoneAsync(dest);                      break;
            }
        });
    }
}
