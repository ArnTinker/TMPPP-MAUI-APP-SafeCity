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
    [ObservableProperty] private string           _detectedAddress  = "Tap 📍 Detect to set location";
    [ObservableProperty] private bool             _isAnonymous;
    [ObservableProperty] private ObservableCollection<string> _attachedMedia = [];
    [ObservableProperty] private string _submitError  = string.Empty;
    [ObservableProperty] private bool   _locationDetected;   // true once any coord is available

    private double _lat, _lon;

    public Array IncidentTypes  => Enum.GetValues(typeof(IncidentType));
    public Array SeverityLevels => Enum.GetValues(typeof(IncidentSeverity));

    public ReportViewModel(
        IReportingFacade facade, IIncidentRepository repo,
        ILocationService location, IMediaService media,
        SessionManager session, CommandHistory history)
    {
        _facade   = facade; _repo = repo;
        _location = location; _media = media;
        _session  = session; _history = history;
    }

    // ── Detect location ────────────────────────────────────────────────────

    [RelayCommand]
    private async Task DetectLocationAsync()
    {
        IsBusy      = true;
        SubmitError = string.Empty;

        var result = await _location.GetCurrentLocationAsync();

        if (result.Status == LocationStatus.PermissionDenied)
        {
            SubmitError = result.ErrorMessage ?? "Location permission denied.";

            // Permanently denied → deep-link to app settings
            if (result.ErrorMessage?.Contains("Settings") == true)
                await TryOpenAppSettingsAsync();

            IsBusy = false;
            return;
        }

        if (result.Status == LocationStatus.GpsOff)
        {
            SubmitError = result.ErrorMessage ?? "GPS is off.";
            IsBusy      = false;
            return;
        }

        // Success OR Unavailable-with-fallback: both have usable coords
        _lat = result.Lat;
        _lon = result.Lon;
        LocationDetected = true;

        DetectedAddress = result.IsSuccess
            ? await _location.ReverseGeocodeAsync(_lat, _lon)
            : $"Fallback: {_lat:F4}, {_lon:F4}";

        if (!result.IsSuccess && result.ErrorMessage is not null)
            SubmitError = result.ErrorMessage;  // show the fallback warning

        IsBusy = false;
    }

    // ── Attach photo (gallery) ─────────────────────────────────────────────

    [RelayCommand]
    private async Task AttachPhotoAsync()
    {
        if (AttachedMedia.Count >= AppConfig.MaxMediaAttachments) return;
        SubmitError = string.Empty;
        var path = await _media.PickPhotoAsync();
        if (path is not null) AttachedMedia.Add(path);
    }

    // ── Capture photo (camera) ─────────────────────────────────────────────

    [RelayCommand]
    private async Task AttachCameraAsync()
    {
        if (AttachedMedia.Count >= AppConfig.MaxMediaAttachments) return;
        SubmitError = string.Empty;

        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            SubmitError = "Camera permission denied.";
            return;
        }

        var path = await _media.CapturePhotoAsync();
        if (path is not null) AttachedMedia.Add(path);
    }

    // ── Submit ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task SubmitAsync()
    {
        SubmitError = string.Empty;

        if (!LocationDetected)
        {
            SubmitError = "Tap '📍 Detect' to set your location first.";
            return;
        }

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
                SelectedType, SelectedSeverity, Description, IsAnonymous,
                AttachedMedia);
            await _history.ExecuteAsync(cmd);

            // Reset form for next use
            Description      = string.Empty;
            DetectedAddress  = "Tap 📍 Detect to set location";
            LocationDetected = false;
            _lat = _lon = 0;
            AttachedMedia.Clear();

            // Navigate to News so the user sees their new incident immediately
            await Shell.Current.GoToAsync("//NewsPage");
        }
        catch (Exception ex)
        {
            // Write full trace to file and show on screen — never swallow silently.
            CrashLogger.Log("ReportViewModel.SubmitAsync", ex);
            SubmitError = ex.Message;
            await Shell.Current.DisplayAlertAsync(
                "Submit Error",
                ex.ToString(),   // type + message + full stack trace
                "OK");
        }
        finally { IsBusy = false; }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static async Task TryOpenAppSettingsAsync()
    {
        try { AppInfo.ShowSettingsUI(); }
        catch
        {
            await Shell.Current.DisplayAlertAsync(
                "Permission Required",
                "Open Settings → Apps → SafeCity → Permissions → Location and set to 'Allow'.",
                "OK");
        }
    }
}
