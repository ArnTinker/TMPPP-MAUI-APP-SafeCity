using SafeCity.Models;
using SQLite;

namespace SafeCity.Data;

public class DatabaseContext
{
    private SQLiteAsyncConnection? _connection;
    private readonly string _dbPath;

    public DatabaseContext()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "safecity.db");
    }

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is null)
        {
            _connection = new SQLiteAsyncConnection(_dbPath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
            await _connection.CreateTableAsync<Incident>();
            await _connection.CreateTableAsync<Report>();
            await _connection.CreateTableAsync<User>();
            await _connection.CreateTableAsync<AlertZone>();
        }
        return _connection;
    }
}
