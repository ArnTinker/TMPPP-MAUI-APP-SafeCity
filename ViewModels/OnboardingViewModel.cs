using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SafeCity.ViewModels;

public partial class OnboardingViewModel : BaseViewModel
{
    [ObservableProperty] private string _headline = "Stay aware.\nStay safe.\nWelcome to SafeCity.";
    [ObservableProperty] private string _subline  = "Real-time safety alerts for your city.";

    [RelayCommand]
    private async Task GetStartedAsync()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
