using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SafeCity.Models;
using SafeCity.Patterns.Behavioral.Observer;
using SafeCity.Patterns.Creational.AbstractFactory;
using SafeCity.Patterns.Creational.FactoryMethod;
using SafeCity.Patterns.Creational.Singleton;
using SafeCity.Patterns.Structural.Decorator;
using SafeCity.Services;

namespace SafeCity.ViewModels;

public partial class AlertsViewModel : BaseViewModel, IIncidentObserver
{
    private readonly SessionManager         _session;
    private readonly IIncidentCreator       _factory;
    private readonly IncidentAlertPublisher _publisher;

    [ObservableProperty] private ObservableCollection<Incident> _recentAlerts = [];
    [ObservableProperty] private bool _isPremium;

    public AlertsViewModel(SessionManager session, IIncidentCreator factory,
        IncidentAlertPublisher publisher)
    {
        _session   = session;
        _factory   = factory;
        _publisher = publisher;
        _publisher.Subscribe(this);
        _isPremium = _session.CurrentUser?.IsPremium ?? false;
    }

    [RelayCommand]
    private void CustomizeAlerts()
    {
        // Premium gate stub
        if (!IsPremium)
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            _ = page?.DisplayAlertAsync("Premium Feature",
                "Upgrade to Premium to customise alert zones.", "OK");
        }
    }

    public void OnIncidentPublished(Incident incident)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var hydrated = _factory.Hydrate(incident);
            RecentAlerts.Insert(0, hydrated);

            // Build the right alert style via Abstract Factory, then decorate via Decorator
            IAlertStyleFactory styleFactory = incident.Severity >= Enums.IncidentSeverity.High
                ? new CriticalAlertFactory()
                : new InfoAlertFactory();

            var bundle = styleFactory.CreateBundle();
            var title  = styleFactory.CreateBannerTitle(incident.Title);
            var body   = styleFactory.CreateBannerBody(incident.Address, 0);

            IAppNotification notification = new BaseNotification(title, body);
            if (bundle.UseVibration) notification = new VibrationNotificationDecorator(notification);
            notification = new SoundNotificationDecorator(notification, bundle.SoundKey);
            if (bundle.PriorityLevel >= 8) notification = new PriorityNotificationDecorator(notification);

            notification.Show();
        });
    }
}
