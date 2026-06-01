using SafeCity.Views;

namespace SafeCity;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register modal routes (not in TabBar)
        Routing.RegisterRoute(nameof(OnboardingPage),        typeof(OnboardingPage));
        Routing.RegisterRoute(nameof(LoginPage),             typeof(LoginPage));
        Routing.RegisterRoute(nameof(CreateProfilePage),     typeof(CreateProfilePage));
        Routing.RegisterRoute(nameof(LocationPermissionPage),typeof(LocationPermissionPage));
        Routing.RegisterRoute(nameof(ReportPage),            typeof(ReportPage));
        Routing.RegisterRoute(nameof(AssistantPage),         typeof(AssistantPage));
    }
}
