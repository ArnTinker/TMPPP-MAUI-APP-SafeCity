using SafeCity.Models;

namespace SafeCity.Data;

public class ReportRepository : IReportRepository
{
    private readonly DatabaseContext _ctx;

    public ReportRepository(DatabaseContext ctx) => _ctx = ctx;

    public async Task<List<Report>> GetAllAsync()
    {
        var db = await _ctx.GetConnectionAsync();
        return await db.Table<Report>().OrderByDescending(r => r.SubmittedAt).ToListAsync();
    }

    public async Task<Report?> GetByIdAsync(int id)
    {
        var db = await _ctx.GetConnectionAsync();
        return await db.FindAsync<Report>(id);
    }

    public async Task<int> InsertAsync(Report report)
    {
        var db = await _ctx.GetConnectionAsync();
        return await db.InsertAsync(report);
    }

    public async Task<int> UpdateAsync(Report report)
    {
        var db = await _ctx.GetConnectionAsync();
        return await db.UpdateAsync(report);
    }
}
