using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SafeCity.Services;

namespace SafeCity.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    [ObservableProperty] private string _phoneOrEmail = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;

    public LoginViewModel(IAuthService auth) => _auth = auth;

    [RelayCommand]
    private async Task ContinueAsync()
    {
        if (string.IsNullOrWhiteSpace(PhoneOrEmail)) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var user = await _auth.LoginAsync(PhoneOrEmail, "stub");
            if (user is null)
                user = await _auth.RegisterAsync(PhoneOrEmail.Split('@')[0], PhoneOrEmail, "stub");
            if (user is not null)
                await Shell.Current.GoToAsync("//CreateProfilePage");
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ContinueWithGoogleAsync() => await ContinueAsync();
}
