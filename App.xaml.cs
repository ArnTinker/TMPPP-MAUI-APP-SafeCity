using SafeCity.Patterns.Creational.Singleton;
using SafeCity.ViewModels;

namespace SafeCity;

public partial class App : Application
{
    private readonly SessionManager _session;

    public App(SessionManager session)
    {
        _session = session;

        // ── Global crash capture ── register before InitializeComponent so any
        // XAML parse error during startup is caught.
        RegisterCrashHandlers();

        InitializeComponent();
    }

    private static void RegisterCrashHandlers()
    {
        // Catches synchronous unhandled exceptions on any thread.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            CrashLogger.Log("AppDomain.UnhandledException",
                            e.ExceptionObject as Exception,
                            fatal: e.IsTerminating);

        // Catches exceptions from unawaited Tasks that are garbage-collected.
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            CrashLogger.Log("TaskScheduler.UnobservedTaskException", e.Exception);
            e.SetObserved(); // stop the process from terminating
        };

#if ANDROID
        // Catches exceptions that bubble through the Java/JNI boundary —
        // required for MAUI XAML parse errors, RecyclerView layout crashes, etc.
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (_, e) =>
        {
            CrashLogger.Log("Android.UnhandledExceptionRaiser", e.Exception);
            e.Handled = true; // keep process alive so the log can be read
        };
#endif
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Eagerly construct all three singleton ViewModels so they subscribe to
        // IncidentAlertPublisher before the user can submit a report.
        //
        // Without this, MAUI DI creates singletons lazily (only on first tab visit).
        // If the user goes straight to "Go Live" without ever opening Alerts or Map,
        // those VMs have never been constructed, never subscribed, and Publish() fires
        // into an empty observer list — incidents reach News but not Alerts or the map.
        var svcs = IPlatformApplication.Current!.Services;
        svcs.GetRequiredService<NewsViewModel>();
        svcs.GetRequiredService<AlertsViewModel>();
        svcs.GetRequiredService<HomeViewModel>();

        return new Window(new AppShell());
    }
}
