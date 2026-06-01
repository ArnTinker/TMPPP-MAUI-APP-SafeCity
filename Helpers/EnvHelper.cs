using DotNetEnv;

namespace SafeCity.Helpers;

public static class EnvHelper
{
    private static bool _loaded;

    public static void Load()
    {
        if (_loaded) return;
        _loaded = true;
        try
        {
            // On Android, .env lives in AppDataDirectory (copied from Raw assets on first run)
            var envPath = Path.Combine(FileSystem.AppDataDirectory, ".env");
            if (File.Exists(envPath))
                Env.Load(envPath);
        }
        catch { /* .env is optional */ }
    }

    public static string? Get(string key) => Environment.GetEnvironmentVariable(key);
}
