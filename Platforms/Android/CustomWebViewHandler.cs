using System.Diagnostics;
using Microsoft.Maui.Handlers;

namespace SafeCity.Platforms.Android;

/// <summary>
/// Overrides the default MAUI WebView handler to ensure JavaScript and DOM storage
/// are enabled — both required for MapLibre GL JS to render tiles.
/// </summary>
public class CustomWebViewHandler : WebViewHandler
{
    protected override void ConnectHandler(global::Android.Webkit.WebView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Settings.JavaScriptEnabled        = true;
        platformView.Settings.DomStorageEnabled        = true;
        platformView.Settings.LoadsImagesAutomatically = true;
        platformView.Settings.MixedContentMode         = global::Android.Webkit.MixedContentHandling.AlwaysAllow;

        // Enable remote debugging via chrome://inspect so we can see JS console errors.
        global::Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
        Debug.WriteLine("[SC-MAP] WebView debugging enabled");
    }
}
