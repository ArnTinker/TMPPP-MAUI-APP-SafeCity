using SafeCity.Views;

namespace SafeCity;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register pushed routes (pages NOT already in the TabBar).
        // ReportPage is a TabBar tab — registering it here again as a pushed route
        // creates a duplicate "ReportPage" global route and causes Shell nav crashes.
        Routing.RegisterRoute(nameof(OnboardingPage),        typeof(OnboardingPage));
        Routing.RegisterRoute(nameof(LoginPage),             typeof(LoginPage));
        Routing.RegisterRoute(nameof(CreateProfilePage),     typeof(CreateProfilePage));
        Routing.RegisterRoute(nameof(LocationPermissionPage),typeof(LocationPermissionPage));
        Routing.RegisterRoute(nameof(AssistantPage),         typeof(AssistantPage));
        Routing.RegisterRoute(nameof(IncidentDetailPage),    typeof(IncidentDetailPage));
    }
}
