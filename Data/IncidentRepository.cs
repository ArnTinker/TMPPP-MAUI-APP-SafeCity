using SafeCity.Models;

namespace SafeCity.Data;

public class IncidentRepository : IIncidentRepository
{
    private readonly DatabaseContext _ctx;

    public IncidentRepository(DatabaseContext ctx) => _ctx = ctx;

    public async Task<List<Incident>> GetAllAsync()
    {
        var db = await _ctx.GetConnectionAsync();
        return await db.Table<Incident>().OrderByDescending(i => i.CreatedAt).ToListAsync();
    }

    public async Task<List<Incident>> GetNearbyAsync(double lat, double lon, double radiusKm)
    {
        var all = await GetAllAsync();
        return all.Where(i => HaversineKm(lat, lon, i.Latitude, i.Longitude) <= radiusKm).ToList();
    }

    public async Task<Incident?> GetByIdAsync(int id)
    {
        var db = await _ctx.GetConnectionAsync();
        return await db.FindAsync<Incident>(id);
    }

    public async Task<int> InsertAsync(Incident incident)
    {
        var db = await _ctx.GetConnectionAsync();
        return await db.InsertAsync(incident);
    }

    public async Task<int> UpdateAsync(Incident incident)
    {
        incident.UpdatedAt = DateTime.UtcNow;
        var db = await _ctx.GetConnectionAsync();
        return await db.UpdateAsync(incident);
    }

    public async Task<int> DeleteAsync(int id)
    {
        var db = await _ctx.GetConnectionAsync();
        return await db.DeleteAsync<Incident>(id);
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
