using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SafeCity.Data;
using SafeCity.Enums;
using SafeCity.Patterns.Behavioral.Command;
using SafeCity.Patterns.Creational.Singleton;
using SafeCity.Patterns.Structural.Facade;
using SafeCity.Services;

namespace SafeCity.ViewModels;

public partial class ReportViewModel : BaseViewModel
{
    private readonly IReportingFacade    _facade;
    private readonly IIncidentRepository _repo;
    private readonly ILocationService    _location;
    private readonly IMediaService       _media;
    private readonly SessionManager      _session;
    private readonly CommandHistory      _history;

    [ObservableProperty] private IncidentType     _selectedType     = IncidentType.Crime;
    [ObservableProperty] private IncidentSeverity _selectedSeverity = IncidentSeverity.Medium;
    [ObservableProperty] private string           _description      = string.Empty;
    [ObservableProperty] private string           _detectedAddress  = "Tap to detect location…";
    [ObservableProperty] private bool             _isAnonymous;
    [ObservableProperty] private ObservableCollection<string> _attachedMedia = [];
    [ObservableProperty] private string _submitError = string.Empty;

    private double _lat, _lon;

    public Array IncidentTypes    => Enum.GetValues(typeof(IncidentType));
    public Array SeverityLevels   => Enum.GetValues(typeof(IncidentSeverity));

    public ReportViewModel(
        IReportingFacade facade, IIncidentRepository repo,
        ILocationService location, IMediaService media,
        SessionManager session, CommandHistory history)
    {
        _facade   = facade; _repo = repo;
        _location = location; _media = media;
        _session  = session; _history = history;
    }

    [RelayCommand]
    private async Task DetectLocationAsync()
    {
        IsBusy = true;
        var loc = await _location.GetCurrentLocationAsync();
        if (loc.HasValue)
        {
            _lat = loc.Value.Lat;
            _lon = loc.Value.Lon;
            DetectedAddress = await _location.ReverseGeocodeAsync(_lat, _lon);
        }
        IsBusy = false;
    }

    [RelayCommand]
    private async Task AttachPhotoAsync()
    {
        if (AttachedMedia.Count >= AppConfig.MaxMediaAttachments) return;
        var path = await _media.PickPhotoAsync();
        if (path is not null) AttachedMedia.Add(path);
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        SubmitError = string.Empty;
        if (string.IsNullOrWhiteSpace(Description) && SelectedType != IncidentType.GoodVibes)
        {
            SubmitError = "Please add a description.";
            return;
        }
        IsBusy = true;
        try
        {
            var cmd = new ReportIncidentCommand(
                _facade, _repo,
                _lat, _lon, DetectedAddress,
                SelectedType, SelectedSeverity, Description, IsAnonymous);
            await _history.ExecuteAsync(cmd);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) { SubmitError = ex.Message; }
        finally { IsBusy = false; }
    }
}
