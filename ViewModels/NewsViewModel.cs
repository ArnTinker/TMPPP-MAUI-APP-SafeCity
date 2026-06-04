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
using SafeCity.Patterns.Structural.Proxy;

namespace SafeCity.ViewModels;

public partial class NewsViewModel : BaseViewModel, IIncidentObserver
{
    private readonly IIncidentRepository    _repo;
    private readonly IIncidentCreator       _factory;
    private readonly SessionManager         _session;
    private readonly CommandHistory         _history;
    private readonly IncidentAlertPublisher _publisher;
    private readonly IMediaLoader           _media;

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
        IncidentAlertPublisher publisher, IMediaLoader media)
    {
        _repo      = repo;
        _factory   = factory;
        _session   = session;
        _history   = history;
        _publisher = publisher;
        _media     = media;
        _activeSortStrategy = _strategies[0];
        _publisher.Subscribe(this);
    }

    partial void OnActiveSortStrategyChanged(IFeedSortStrategy value) => ApplySort();

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        var raw      = await _repo.GetAllAsync();
        var hydrated = raw.Select(_factory.Hydrate).ToList();
        await PreloadThumbnailsAsync(hydrated);
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

    [RelayCommand]
    private async Task SelectIncidentAsync(Incident incident)
    {
        await Shell.Current.GoToAsync($"IncidentDetailPage?id={incident.Id}");
    }

    /// <summary>Observer callback — new incident arrives immediately without manual refresh.</summary>
    public void OnIncidentPublished(Incident incident)
    {
        // Load thumbnail via proxy then insert on main thread
        _ = LoadThumbnailAndInsertAsync(_factory.Hydrate(incident));
    }

    private async Task LoadThumbnailAndInsertAsync(Incident incident)
    {
        await LoadThumbnailAsync(incident);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Incidents.Insert(0, incident);
            TopStory = Incidents.FirstOrDefault();
        });
    }

    /// <summary>
    /// Routes each incident's first photo through CachedIncidentMediaProxy so the
    /// proxy cache is populated before the cell is rendered. Keeps the Proxy in the
    /// display path per the Proxy pattern requirement.
    /// </summary>
    private async Task PreloadThumbnailsAsync(IEnumerable<Incident> incidents)
    {
        foreach (var inc in incidents)
            await LoadThumbnailAsync(inc);
    }

    private async Task LoadThumbnailAsync(Incident incident)
    {
        var path = incident.FirstMediaPath;
        if (path is null) return;

        var stream = await _media.LoadAsync(path);
        if (stream is null) return;

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var bytes = ms.ToArray();
        incident.ThumbnailSource = ImageSource.FromStream(() => new MemoryStream(bytes));
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
