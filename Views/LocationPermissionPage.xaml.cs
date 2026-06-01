using SafeCity.ViewModels;

namespace SafeCity.Views;

public partial class LocationPermissionPage : ContentPage
{
    public LocationPermissionPage(LocationPermissionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
