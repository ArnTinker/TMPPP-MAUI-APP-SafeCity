using Microsoft.Extensions.Logging;
using SafeCity.Data;
using SafeCity.Helpers;
using SafeCity.Patterns.Behavioral.Command;
using SafeCity.Patterns.Behavioral.Observer;
using SafeCity.Patterns.Creational.Builder;
using SafeCity.Patterns.Creational.FactoryMethod;
using SafeCity.Patterns.Creational.Singleton;
using SafeCity.Patterns.Structural.Adapter;
using SafeCity.Patterns.Structural.Facade;
using SafeCity.Patterns.Structural.Proxy;
using SafeCity.Services;
using SafeCity.ViewModels;
using SafeCity.Views;

namespace SafeCity;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        EnvHelper.Load();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf",   "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf",  "OpenSansSemibold");
            });

        var s = builder.Services;

        // ── HTTP clients ──
        s.AddHttpClient("Gemini");
        s.AddHttpClient("Media");

        // ── Database ──
        s.AddSingleton<DatabaseContext>();

        // ── Repositories (Dependency Inversion: VMs depend on interfaces) ──
        s.AddSingleton<IIncidentRepository, IncidentRepository>();
        s.AddSingleton<IReportRepository,   ReportRepository>();
        s.AddSingleton<IUserRepository,     UserRepository>();

        // ── PATTERN 1: Singleton — one session for the lifetime of the app ──
        s.AddSingleton<SessionManager>();

        // ── PATTERN 2: Factory Method ──
        s.AddSingleton<IIncidentCreator, IncidentFactory>();

        // ── PATTERN 4: Builder ──
        s.AddTransient<IReportBuilder, ReportBuilder>();

        // ── PATTERN 9: Observer — singleton publisher shared by all VMs ──
        s.AddSingleton<IncidentAlertPublisher>();

        // ── PATTERN 12: Command — shared history for undo ──
        s.AddSingleton<CommandHistory>();

        // ── PATTERN 5: Facade ──
        s.AddScoped<IReportingFacade, ReportingFacade>();

        // ── PATTERN 6: Adapter — Gemini assistant ──
        s.AddSingleton<IAssistant>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var key     = EnvHelper.Get("GEMINI_API_KEY");
            return new GeminiAssistantAdapter(factory, key);
        });

        // ── PATTERN 8: Proxy — media loader with caching ──
        s.AddSingleton<IMediaLoader>(sp =>
        {
            var real = new RealMediaLoader(sp.GetRequiredService<IHttpClientFactory>());
            return new CachedIncidentMediaProxy(real);
        });

        // ── Services ──
        s.AddSingleton<ILocationService, LocationService>();
        s.AddSingleton<IMediaService,    MediaService>();
        s.AddSingleton<IAuthService,     AuthService>();

        // ── ViewModels (Transient = fresh VM per navigation) ──
        s.AddTransient<OnboardingViewModel>();
        s.AddTransient<LoginViewModel>();
        s.AddTransient<CreateProfileViewModel>();
        s.AddTransient<LocationPermissionViewModel>();
        s.AddSingleton<NewsViewModel>();
        s.AddSingleton<HomeViewModel>();
        s.AddSingleton<AlertsViewModel>();
        s.AddTransient<ProfileViewModel>();
        s.AddTransient<ReportViewModel>();
        s.AddTransient<AssistantViewModel>();

        // ── Views ──
        s.AddTransient<OnboardingPage>();
        s.AddTransient<LoginPage>();
        s.AddTransient<CreateProfilePage>();
        s.AddTransient<LocationPermissionPage>();
        s.AddSingleton<NewsPage>();
        s.AddSingleton<HomePage>();
        s.AddSingleton<AlertsPage>();
        s.AddTransient<ProfilePage>();
        s.AddTransient<ReportPage>();
        s.AddTransient<AssistantPage>();

        s.AddSingleton<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
