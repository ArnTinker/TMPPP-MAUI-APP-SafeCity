namespace SafeCity.Services;

public class LocationService : ILocationService
{
    public async Task<(double Lat, double Lon)?> GetCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                return null;

            var location = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));
            return location is null ? null : (location.Latitude, location.Longitude);
        }
        catch
        {
            return null;
        }
    }

    public async Task<string> ReverseGeocodeAsync(double lat, double lon)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(lat, lon);
            var p = placemarks?.FirstOrDefault();
            if (p is null) return $"{lat:F4}, {lon:F4}";
            return $"{p.Thoroughfare} {p.SubThoroughfare}, {p.Locality}".Trim(' ', ',');
        }
        catch
        {
            return $"{lat:F4}, {lon:F4}";
        }
    }
}
