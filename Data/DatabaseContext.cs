using SafeCity.Models;
using SQLite;

namespace SafeCity.Data;

/// <summary>
/// Owns the single SQLiteAsyncConnection for the app.
/// Registered as AddSingleton so every repository shares the same connection —
/// do not resolve this from a transient or scoped context.
///
/// Schema: one table per model, created idempotently on first access.
/// Incident subtypes (AccidentIncident, FireIncident…) share the Incidents table;
/// their domain behaviour is restored via IIncidentCreator.Hydrate() after read.
/// </summary>
public class DatabaseContext
{
    private readonly string _dbPath;

    // Single lazy init task: every caller awaits the same Task, so table creation
    // runs exactly once and the connection is never handed out before its schema
    // exists. Guards against the concurrent-first-access "no such table" race.
    private readonly SemaphoreSlim _gate = new(1, 1);
    private SQLiteAsyncConnection? _connection;

#if DEBUG
    // Set true to delete the SQLite file on the next cold start — useful during
    // development when the schema changes. Reset to false after one clean run.
    private const bool ForceSchemaReset = true;
#endif

    public DatabaseContext()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "safecity.db");
    }

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null) return _connection;

        await _gate.WaitAsync();
        try
        {
            if (_connection is not null) return _connection;

#if DEBUG
            if (ForceSchemaReset && File.Exists(_dbPath))
                File.Delete(_dbPath);
#endif

            var connection = new SQLiteAsyncConnection(_dbPath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            // CreateTableAsync is idempotent — safe to call on every cold start.
            // Incident is the single table for all incident subtypes (Factory Method
            // domain objects are flattened to / rehydrated from this one table).
            await connection.CreateTableAsync<Incident>();
            await connection.CreateTableAsync<Report>();
            await connection.CreateTableAsync<User>();
            await connection.CreateTableAsync<AlertZone>();

            // Publish only after the schema is fully built.
            _connection = connection;
            return _connection;
        }
        finally
        {
            _gate.Release();
        }
    }
}
