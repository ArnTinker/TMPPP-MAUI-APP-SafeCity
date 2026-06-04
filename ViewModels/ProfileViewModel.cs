using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SafeCity.Patterns.Behavioral.Command;
using SafeCity.Patterns.Creational.Singleton;
using SafeCity.Data;
using SafeCity.Services;

namespace SafeCity.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly SessionManager  _session;
    private readonly IAuthService    _auth;
    private readonly CommandHistory  _history;
    private readonly IUserRepository _users;

    [ObservableProperty] private string _displayName  = "Guest";
    [ObservableProperty] private string _email        = string.Empty;
    [ObservableProperty] private bool   _isPremium;
    [ObservableProperty] private int    _reportCount;
    [ObservableProperty] private int    _upvotesReceived;
    [ObservableProperty] private string _achievementBadge = "Community Helper";

    public ProfileViewModel(SessionManager session, IAuthService auth,
        CommandHistory history, IUserRepository users)
    {
        _session = session;
        _auth    = auth;
        _history = history;
        _users   = users;
        Refresh();
    }

    private void Refresh()
    {
        if (_session.CurrentUser is null) return;
        DisplayName     = _session.CurrentUser.Username;
        Email           = _session.CurrentUser.Email;
        IsPremium       = _session.CurrentUser.IsPremium;
        ReportCount     = _session.CurrentUser.ReportCount;
        UpvotesReceived = _session.CurrentUser.UpvotesReceived;
        AchievementBadge = ReportCount switch
        {
            >= 50  => "Community Legend",
            >= 20  => "Community Champion",
            >= 5   => "Community Helper",
            _      => "New Member"
        };
    }

    [RelayCommand]
    private async Task JoinSafetyNetworkAsync()
    {
        var cmd = new JoinSafetyNetworkCommand(_session, _users);
        await _history.ExecuteAsync(cmd);
        Refresh();
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        _history.Clear();
        await Shell.Current.GoToAsync("//OnboardingPage");
    }

    [RelayCommand]
    private async Task ViewCrashLogAsync()
    {
        var log = CrashLogger.Read();
        await Shell.Current.DisplayAlertAsync("Crash Log", log, "Close");
    }

    [RelayCommand]
    private void ClearCrashLog()
    {
        CrashLogger.Clear();
        _ = Shell.Current.DisplayAlertAsync("Crash Log", "Log cleared.", "OK");
    }
}
