using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SafeCity.Services.Maps;

namespace SafeCity.Views.Controls;

public partial class MapMdView : ContentView
{
    private readonly MapMdProvider _provider;
    private bool _loaded;

    public MapMdView()
    {
        InitializeComponent();
        _provider = IPlatformApplication.Current!.Services
            .GetRequiredService<MapMdProvider>();
        _provider.MapReady    += OnProviderReady;
        _provider.MapLoadError += OnProviderError;
    }

    // ── Lifecycle ──────────────────────────────────────────────────

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is not null && !_loaded)
        {
            _loaded = true;
            _ = LoadMapAsync();
        }
        else if (Handler is null)
        {
            _provider.Detach();
        }
    }

    // ── Map loading ────────────────────────────────────────────────

    private async Task LoadMapAsync()
    {
        var token = AppConfig.MapMdToken ?? string.Empty;
        Debug.WriteLine($"[SC-MAP] LoadMapAsync: token={(string.IsNullOrEmpty(token) ? "EMPTY" : $"{token[..8]}…")}");

        if (string.IsNullOrEmpty(token))
        {
            PlaceholderLabel.Text = "Set MAPMD_TOKEN in .env to enable the map.";
            Debug.WriteLine("[SC-MAP] Aborted — no token.");
            return;
        }

        var html = await GetMapHtmlAsync(token);
        Debug.WriteLine($"[SC-MAP] HTML loaded ({html.Length} chars), attaching WebView");
        _provider.Attach(MapWebView);
        // BaseUrl = "https://map.md/" makes the WebView treat the inline HTML as
        // same-origin with map.md tiles. Without it the origin is null (about:blank),
        // which causes map.md to reject the Authorization-header CORS preflight.
        MapWebView.Source = new HtmlWebViewSource { Html = html, BaseUrl = "https://map.md/" };
    }

    private static async Task<string> GetMapHtmlAsync(string token)
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("mapmd.html");
            using var reader = new StreamReader(stream);
            var template = await reader.ReadToEndAsync();
            return template.Replace("{{MAPMD_TOKEN}}", token);
        }
        catch
        {
            return FallbackHtml();
        }
    }

    private static string FallbackHtml() =>
        """
        <!DOCTYPE html><html>
        <body style="background:#000;color:#FFD60A;display:flex;align-items:center;
                     justify-content:center;height:100vh;font-family:sans-serif;font-size:14px">
          <p>Map could not be loaded.</p>
        </body></html>
        """;

    // ── Provider events → UI ───────────────────────────────────────

    private void OnProviderReady()
    {
        Debug.WriteLine("[SC-MAP] OnProviderReady — map tiles loaded ✓");
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Placeholder.IsVisible      = false;
            PlaceholderLabel.IsVisible = false;
            MapWebView.IsVisible       = true;
        });
    }

    private void OnProviderError(string message)
    {
        Debug.WriteLine($"[SC-MAP] OnProviderError: {message}");
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PlaceholderLabel.Text = $"Map error: {message}";
        });
    }
}
