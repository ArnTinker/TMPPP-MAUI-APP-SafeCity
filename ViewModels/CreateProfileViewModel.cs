using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SafeCity.Data;
using SafeCity.Patterns.Creational.Singleton;

namespace SafeCity.ViewModels;

public partial class CreateProfileViewModel : BaseViewModel
{
    private readonly IUserRepository _users;
    private readonly SessionManager _session;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _usernameStatus = string.Empty;
    [ObservableProperty] private bool   _usernameAvailable;

    public CreateProfileViewModel(IUserRepository users, SessionManager session)
    {
        _users   = users;
        _session = session;
    }

    partial void OnUsernameChanged(string value) => _ = CheckUsernameAsync(value);

    private async Task CheckUsernameAsync(string username)
    {
        if (username.Length < 3) { UsernameStatus = "Too short"; UsernameAvailable = false; return; }
        var available = await _users.IsUsernameAvailableAsync(username);
        UsernameAvailable = available;
        UsernameStatus    = available ? "✓ Username available" : "✗ Username taken";
    }

    [RelayCommand]
    private async Task ContinueAsync()
    {
        if (!UsernameAvailable || _session.CurrentUser is null) return;
        _session.CurrentUser.Username = Username;
        await _users.UpdateAsync(_session.CurrentUser);
        await Shell.Current.GoToAsync("//LocationPermissionPage");
    }
}
