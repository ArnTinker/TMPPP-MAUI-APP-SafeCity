using System.Diagnostics;
using System.Text;

namespace SafeCity;

/// <summary>
/// Static crash-capture helper. Appends structured entries to a persistent file
/// in AppDataDirectory so they survive across sessions and can be read on-device.
/// </summary>
public static class CrashLogger
{
    // Evaluated lazily so FileSystem is ready before first access.
    private static string LogPath =>
        Path.Combine(FileSystem.AppDataDirectory, "crash_log.txt");

    public static void Log(string source, Exception? ex, bool fatal = false)
    {
        try
        {
            var entry = BuildEntry(source, ex, fatal);
            Debug.WriteLine(entry);
            File.AppendAllText(LogPath, entry);
        }
        catch { /* never crash inside the crash handler */ }
    }

    public static string Read()
    {
        try
        {
            if (!File.Exists(LogPath)) return "(no crash_log.txt found)";
            var text = File.ReadAllText(LogPath);
            // Return the last 4 000 chars so it fits in a DisplayAlert
            return text.Length > 4000 ? "…(truncated)\n\n" + text[^4000..] : text;
        }
        catch (Exception ex) { return $"Error reading log: {ex.Message}"; }
    }

    public static void Clear()
    {
        try { if (File.Exists(LogPath)) File.Delete(LogPath); }
        catch { }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string BuildEntry(string source, Exception? ex, bool fatal)
    {
        var sb = new StringBuilder();
        sb.AppendLine("══════════════════════════════════════════════");
        sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]  {source}{(fatal ? "  [FATAL]" : "")}");

        if (ex is null)
        {
            sb.AppendLine("  (no exception object)");
        }
        else
        {
            AppendException(sb, ex, "");
        }

        sb.AppendLine();
        return sb.ToString();
    }

    private static void AppendException(StringBuilder sb, Exception ex, string indent)
    {
        sb.AppendLine($"{indent}Type   : {ex.GetType().FullName}");
        sb.AppendLine($"{indent}Message: {ex.Message}");

        if (ex.StackTrace is { } st)
        {
            sb.AppendLine($"{indent}Stack  :");
            foreach (var line in st.Split('\n'))
                sb.AppendLine($"{indent}  {line.TrimEnd()}");
        }

        if (ex.InnerException is not null)
        {
            sb.AppendLine($"{indent}── InnerException ──");
            AppendException(sb, ex.InnerException, indent + "  ");
        }

        if (ex is AggregateException agg)
        {
            foreach (var inner in agg.InnerExceptions.Skip(1)) // first is already InnerException
            {
                sb.AppendLine($"{indent}── AggregateInner ──");
                AppendException(sb, inner, indent + "  ");
            }
        }
    }
}
