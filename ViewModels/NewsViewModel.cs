using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SafeCity.Data;
using SafeCity.Models;
using SafeCity.Patterns.Behavioral.Command;
using SafeCity.Patterns.Behavioral.Observer;
using SafeCity.Patterns.Behavioral.Strategy;
using SafeCity.Patterns.Creational.FactoryMethod;
using SafeCity.Patterns.Creational.Singleton;

namespace SafeCity.ViewModels;

public partial class NewsViewModel : BaseViewModel, IIncidentObserver
{
    private readonly IIncidentRepository _repo;
    private readonly IIncidentCreator    _factory;
    private readonly SessionManager      _session;
    private readonly CommandHistory      _history;
    private readonly IncidentAlertPublisher _publisher;

    private readonly List<IFeedSortStrategy> _strategies =
    [
        new ByRecencyStrategy(),
        new ByDistanceStrategy(),
        new BySeverityStrategy(),
        new ByPopularityStrategy()
    ];

    [ObservableProperty] private ObservableCollection<Incident> _incidents = [];
    [ObservableProperty] private IFeedSortStrategy _activeSortStrategy;
    [ObservableProperty] private Incident? _topStory;

    public List<IFeedSortStrategy> SortStrategies => _strategies;

    public NewsViewModel(
        IIncidentRepository repo, IIncidentCreator factory,
        SessionManager session, CommandHistory history,
        IncidentAlertPublisher publisher)
    {
        _repo      = repo;
        _factory   = factory;
        _session   = session;
        _history   = history;
        _publisher = publisher;
        _activeSortStrategy = _strategies[0];
        _publisher.Subscribe(this);
    }

    partial void OnActiveSortStrategyChanged(IFeedSortStrategy value) => ApplySort();

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        var raw = await _repo.GetAllAsync();
        var hydrated = raw.Select(_factory.Hydrate).ToList();
        ApplySort(hydrated);
        IsBusy = false;
    }

    [RelayCommand]
    private async Task UpvoteAsync(Incident incident)
    {
        var cmd = new UpvoteCommand(_repo, incident);
        await _history.ExecuteAsync(cmd);
    }

    [RelayCommand]
    private async Task ShareAsync(Incident incident)
    {
        var cmd = new ShareIncidentCommand(incident);
        await _history.ExecuteAsync(cmd);
    }

    [RelayCommand]
    private async Task UndoLastAsync() => await _history.UndoLastAsync();

    public void OnIncidentPublished(Incident incident)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Incidents.Insert(0, _factory.Hydrate(incident));
            TopStory = Incidents.FirstOrDefault();
        });
    }

    private void ApplySort(List<Incident>? source = null)
    {
        var list = source ?? Incidents.ToList();
        var sorted = ActiveSortStrategy.Sort(list,
            _session.LastKnownLat ?? AppConfig.DefaultLat,
            _session.LastKnownLon ?? AppConfig.DefaultLon).ToList();
        Incidents = new ObservableCollection<Incident>(sorted);
        TopStory  = Incidents.FirstOrDefault();
    }
}
