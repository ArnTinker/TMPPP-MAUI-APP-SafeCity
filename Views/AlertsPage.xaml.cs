using SafeCity.ViewModels;

namespace SafeCity.Views;

public partial class AlertsPage : ContentPage
{
    public AlertsPage(AlertsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
