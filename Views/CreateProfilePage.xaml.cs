using SafeCity.ViewModels;

namespace SafeCity.Views;

public partial class CreateProfilePage : ContentPage
{
    public CreateProfilePage(CreateProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
