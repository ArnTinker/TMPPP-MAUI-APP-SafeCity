using SafeCity.Patterns.Creational.Singleton;

namespace SafeCity;

public partial class App : Application
{
    private readonly SessionManager _session;

    public App(SessionManager session)
    {
        _session = session;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
