using SafeCity.Models;
using SafeCity.Patterns.Creational.FactoryMethod;

namespace SafeCity.Data;

public class IncidentRepository : IIncidentRepository
{
    private readonly DatabaseContext _ctx;
    private readonly IIncidentCreator _factory;

    public IncidentRepository(DatabaseContext ctx, IIncidentCreator factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public async Task<List<Incident>> GetAllAsync()
    {
        var db = await _ctx.GetConnectionAsync();
        var rows = await db.Table<Incident>().OrderByDescending(i => i.CreatedAt).ToListAsync();
        // Reads come back as flat base rows; the factory rehydrates each into its
        // correct subtype using the Type discriminator. Flattening/rehydration stays
        // hidden in the repository so the rest of the app only sees domain Incidents.
        return rows.Select(_factory.Hydrate).ToList();
    }

    public async Task<List<Incident>> GetNearbyAsync(double lat, double lon, double radiusKm)
    {
        var all = await GetAllAsync();
        return all.Where(i => HaversineKm(lat, lon, i.Latitude, i.Longitude) <= radiusKm).ToList();
    }

    public async Task<Incident?> GetByIdAsync(int id)
    {
        var db = await _ctx.GetConnectionAsync();
        var row = await db.FindAsync<Incident>(id);
        return row is null ? null : _factory.Hydrate(row);
    }

    public async Task<int> InsertAsync(Incident incident)
    {
        var db  = await _ctx.GetConnectionAsync();
        var row = ToBaseRow(incident);
        var result = await db.InsertAsync(row);
        incident.Id = row.Id;  
        return result;
    }

    public async Task<int> UpdateAsync(Incident incident)
    {
        incident.UpdatedAt = DateTime.UtcNow;
        var db  = await _ctx.GetConnectionAsync();
        var row = ToBaseRow(incident);
        return await db.UpdateAsync(row);
    }

    public async Task<int> DeleteAsync(int id)
    {
        var db = await _ctx.GetConnectionAsync();
        return await db.DeleteAsync<Incident>(id);
    }
    
    private static Incident ToBaseRow(Incident src) => new()
    {
        Id            = src.Id,
        Title         = src.Title,
        Description   = src.Description,
        Latitude      = src.Latitude,
        Longitude     = src.Longitude,
        Address       = src.Address,
        CreatedAt     = src.CreatedAt,
        UpdatedAt     = src.UpdatedAt,
        Type          = src.Type,
        Severity      = src.Severity,
        State         = src.State,
        UpvoteCount   = src.UpvoteCount,
        ViewCount     = src.ViewCount,
        CommentCount  = src.CommentCount,
        IsAnonymous   = src.IsAnonymous,
        ReporterId    = src.ReporterId,
        MediaUrlsJson = src.MediaUrlsJson,
    };

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
