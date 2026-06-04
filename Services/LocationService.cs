namespace SafeCity.Services;

public class LocationService : ILocationService
{
    public async Task<LocationResult> GetCurrentLocationAsync()
    {
        try
        {
            // ── 1. Check / request runtime permission ───────────────────────────
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Denied)
            {
                // ShouldShowRationale == false after second denial → permanently denied
                var canAsk = Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>();
                var msg = canAsk
                    ? "Location permission denied. Tap 'Detect' again to re-request."
                    : "Location permanently denied — open Settings to enable it.";
                return new LocationResult(LocationStatus.PermissionDenied, ErrorMessage: msg);
            }

            // ── 2. Request live GPS fix ──────────────────────────────────────────
            Location? location = null;
            try
            {
                location = await Geolocation.Default.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));
            }
            catch (FeatureNotEnabledException)
            {
                return new LocationResult(LocationStatus.GpsOff,
                    ErrorMessage: "GPS is off — enable Location in device Settings.");
            }

            // ── 3. Fall back to last-known if live fix timed out ─────────────────
            if (location is null)
                location = await Geolocation.Default.GetLastKnownLocationAsync();

            // ── 4. Still null → emulator or GPS unavailable; use Chișinău default ─
            if (location is null)
                return new LocationResult(LocationStatus.Unavailable,
                    AppConfig.DefaultLat, AppConfig.DefaultLon,
                    ErrorMessage: "Couldn't get a GPS fix — using Chișinău centre as fallback. " +
                                  "On an emulator, set a location in Extended Controls → Location.");

            return new LocationResult(LocationStatus.Success, location.Latitude, location.Longitude);
        }
        catch (PermissionException)
        {
            return new LocationResult(LocationStatus.PermissionDenied,
                ErrorMessage: "Location permission denied. Open Settings to enable it.");
        }
        catch (FeatureNotSupportedException)
        {
            return new LocationResult(LocationStatus.Unavailable,
                AppConfig.DefaultLat, AppConfig.DefaultLon,
                ErrorMessage: "Location is not supported on this device — using Chișinău centre.");
        }
        catch (Exception ex)
        {
            return new LocationResult(LocationStatus.Unavailable,
                AppConfig.DefaultLat, AppConfig.DefaultLon,
                ErrorMessage: $"Location error: {ex.Message}. Using Chișinău centre.");
        }
    }

    public async Task<string> ReverseGeocodeAsync(double lat, double lon)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(lat, lon);
            var p = placemarks?.FirstOrDefault();
            if (p is null) return $"{lat:F4}, {lon:F4}";
            var parts = new[] { p.Thoroughfare, p.SubThoroughfare, p.Locality }
                .Where(s => !string.IsNullOrWhiteSpace(s));
            return string.Join(", ", parts).TrimEnd(',', ' ');
        }
        catch
        {
            return $"{lat:F4}, {lon:F4}";
        }
    }
}
