using SafeCity.Models;

namespace SafeCity.Data;

public interface IReportRepository
{
    Task<List<Report>> GetAllAsync();
    Task<Report?> GetByIdAsync(int id);
    Task<int> InsertAsync(Report report);
    Task<int> UpdateAsync(Report report);
}
