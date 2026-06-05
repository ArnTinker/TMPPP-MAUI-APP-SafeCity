using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SafeCity.Services.Maps;

namespace SafeCity.Views.Controls;

public partial class MapboxMapView : ContentView
{
    private readonly MapboxProvider _provider;
    private bool _loaded;

    public MapboxMapView()
    {
        InitializeComponent();
        _provider = IPlatformApplication.Current!.Services
            .GetRequiredService<MapboxProvider>();
        _provider.MapReady     += OnProviderReady;
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
        var token = AppConfig.MapboxToken ?? string.Empty;
        Debug.WriteLine($"[SC-MAP] LoadMapAsync: token={(string.IsNullOrEmpty(token) ? "EMPTY" : $"{token[..Math.Min(8, token.Length)]}…")}");

        if (string.IsNullOrEmpty(token))
        {
            PlaceholderLabel.Text = "Set MAPBOX_TOKEN in .env to enable the map.";
            Debug.WriteLine("[SC-MAP] Aborted — no token.");
            return;
        }

        var html = await GetMapHtmlAsync(token);
        Debug.WriteLine($"[SC-MAP] HTML loaded ({html.Length} chars), attaching WebView");
        _provider.Attach(MapWebView);
        // A non-null https BaseUrl gives the inline page a real origin so Mapbox
        // GL JS XHR/fetch tile requests behave (api.mapbox.com allows CORS from any origin).
        MapWebView.Source = new HtmlWebViewSource { Html = html, BaseUrl = "https://localhost/" };
    }

    private static async Task<string> GetMapHtmlAsync(string token)
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("mapbox.html");
            using var reader = new StreamReader(stream);
            var template = await reader.ReadToEndAsync();
            return template
                .Replace("{{MAPBOX_TOKEN}}", token)
                .Replace("{{MAPBOX_STYLE}}", AppConfig.MapboxStyleUrl ?? string.Empty);
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
