using CommunityToolkit.Mvvm.Input;
using SafeCity.Patterns.Creational.Singleton;
using SafeCity.Services;

namespace SafeCity.ViewModels;

public partial class LocationPermissionViewModel : BaseViewModel
{
    private readonly ILocationService _location;
    private readonly SessionManager   _session;

    public LocationPermissionViewModel(ILocationService location, SessionManager session)
    {
        _location = location;
        _session  = session;
    }

    [RelayCommand]
    private async Task AllowLocationAsync()
    {
        IsBusy = true;
        var loc = await _location.GetCurrentLocationAsync();
        if (loc.HasValue)
        {
            _session.UpdateLocation(loc.Value.Lat, loc.Value.Lon);
            _session.LocationGranted = true;
        }
        IsBusy = false;
        await Shell.Current.GoToAsync("//MainTabs");
    }

    [RelayCommand]
    private async Task SkipAsync() => await Shell.Current.GoToAsync("//MainTabs");
}
