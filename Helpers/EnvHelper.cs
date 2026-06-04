using DotNetEnv;

namespace SafeCity.Helpers;

public static class EnvHelper
{
    private static bool _loaded;

    public static void Load()
    {
        if (_loaded) return;
        _loaded = true;

        // 1. Prefer .env in AppDataDirectory (user can push a file here for overrides)
        var dataEnv = Path.Combine(FileSystem.AppDataDirectory, ".env");
        if (File.Exists(dataEnv))
        {
            Env.Load(dataEnv);
            return;
        }

        // 2. Fall back to the env file bundled as a Raw asset inside the APK.
        // Android AAPT strips dotfiles, so the bundled copy is named "dotenv"
        // (no leading dot). Try both names for forward/backward compatibility.
        foreach (var name in new[] { "dotenv", ".env" })
        {
            try
            {
                using var stream = FileSystem.OpenAppPackageFileAsync(name).GetAwaiter().GetResult();
                Env.Load(stream);
                return;
            }
            catch { /* try next */ }
        }
    }

    public static string? Get(string key) => Environment.GetEnvironmentVariable(key);
}
