using System.Diagnostics;

namespace SafeCity.Services.Maps;

/// <summary>
/// Adaptee for the Adapter pattern: owns the raw WebView ↔ Mapbox GL JS bridge.
/// Knows nothing about Incident models — that translation is MapboxMapAdapter's job.
/// Queues JS calls that arrive before the map fires 'ready' and replays them on init.
/// </summary>
public sealed class MapboxProvider
{
    private WebView?               _webView;
    private bool                   _mapReady;
    private readonly Queue<string> _pendingJs = new();

    public event Action?         MapReady;
    public event Action<int>?    MarkerTapped;
    public event Action<string>? MapLoadError;

    public bool IsReady => _mapReady;

    // ── Called by MapboxMapView once the WebView is in the visual tree ──

    public void Attach(WebView webView)
    {
        _webView            = webView;
        _webView.Navigating += OnNavigating;
    }

    public void Detach()
    {
        if (_webView is null) return;
        _webView.Navigating -= OnNavigating;
        _webView  = null;
        _mapReady = false;
    }

    // ── Raw JS execution (called by MapboxMapAdapter) ──

    public Task ExecuteAsync(string js)
    {
        if (_webView is null || !_mapReady)
        {
            _pendingJs.Enqueue(js);
            return Task.CompletedTask;
        }
        return MainThread.InvokeOnMainThreadAsync(
            () => _webView.EvaluateJavaScriptAsync(js));
    }

    // ── JS → C# message interception via safecity:// URL scheme ──

    private void OnNavigating(object? sender, WebNavigatingEventArgs e)
    {
        const string scheme = "safecity://";
        Debug.WriteLine($"[SC-MAP] WebView navigating: {e.Url[..Math.Min(120, e.Url.Length)]}");
        if (!e.Url.StartsWith(scheme, StringComparison.OrdinalIgnoreCase)) return;

        e.Cancel = true;
        var path = e.Url[scheme.Length..];

        if (path == "ready")
        {
            _mapReady = true;
            while (_pendingJs.TryDequeue(out var js))
                _ = ExecuteAsync(js);
            MapReady?.Invoke();
        }
        else if (path.StartsWith("marker-tap/", StringComparison.Ordinal) &&
                 int.TryParse(path["marker-tap/".Length..], out var id))
        {
            MarkerTapped?.Invoke(id);
        }
        else if (path.StartsWith("error/", StringComparison.Ordinal))
        {
            var msg = Uri.UnescapeDataString(path["error/".Length..]);
            MapLoadError?.Invoke(msg);
        }
    }
}
