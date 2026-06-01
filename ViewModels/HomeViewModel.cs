using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SafeCity.Data;
using SafeCity.Models;
using SafeCity.Patterns.Behavioral.Observer;
using SafeCity.Patterns.Creational.FactoryMethod;
using SafeCity.Patterns.Creational.Singleton;
using SafeCity.Services;

namespace SafeCity.ViewModels;

public partial class HomeViewModel : BaseViewModel, IIncidentObserver
{
    private readonly IIncidentRepository _repo;
    private readonly IIncidentCreator    _factory;
    private readonly SessionManager      _session;
    private readonly ILocationService    _location;
    private readonly IncidentAlertPublisher _publisher;

    [ObservableProperty] private ObservableCollection<Incident> _nearbyIncidents = [];
    [ObservableProperty] private double  _userLat    = AppConfig.DefaultLat;
    [ObservableProperty] private double  _userLon    = AppConfig.DefaultLon;
    [ObservableProperty] private int     _nearbyCount;
    [ObservableProperty] private string  _nearbyUsers = "—";

    public HomeViewModel(
        IIncidentRepository repo, IIncidentCreator factory,
        SessionManager session, ILocationService location,
        IncidentAlertPublisher publisher)
    {
        _repo      = repo;
        _factory   = factory;
        _session   = session;
        _location  = location;
        _publisher = publisher;
        _publisher.Subscribe(this);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        var loc = await _location.GetCurrentLocationAsync();
        if (loc.HasValue)
        {
            UserLat = loc.Value.Lat;
            UserLon = loc.Value.Lon;
            _session.UpdateLocation(UserLat, UserLon);
        }
        var nearby = await _repo.GetNearbyAsync(UserLat, UserLon, AppConfig.NearbyRadiusKm);
        NearbyIncidents = new ObservableCollection<Incident>(nearby.Select(_factory.Hydrate));
        NearbyCount     = NearbyIncidents.Count;
        IsBusy = false;
    }

    [RelayCommand]
    private async Task GoToReportAsync() =>
        await Shell.Current.GoToAsync("ReportPage");

    public void OnIncidentPublished(Incident incident)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            NearbyIncidents.Insert(0, _factory.Hydrate(incident));
            NearbyCount = NearbyIncidents.Count;
        });
    }
}
