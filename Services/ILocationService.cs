namespace SafeCity.Services;

public interface ILocationService
{
    Task<(double Lat, double Lon)?> GetCurrentLocationAsync();
    Task<string> ReverseGeocodeAsync(double lat, double lon);
}
