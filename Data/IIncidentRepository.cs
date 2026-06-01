using SafeCity.Models;

namespace SafeCity.Data;

public interface IIncidentRepository
{
    Task<List<Incident>> GetAllAsync();
    Task<List<Incident>> GetNearbyAsync(double lat, double lon, double radiusKm);
    Task<Incident?> GetByIdAsync(int id);
    Task<int> InsertAsync(Incident incident);
    Task<int> UpdateAsync(Incident incident);
    Task<int> DeleteAsync(int id);
}
