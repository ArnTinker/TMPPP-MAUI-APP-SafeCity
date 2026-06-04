using SafeCity.ViewModels;

namespace SafeCity.Views;

public partial class IncidentDetailPage : ContentPage
{
    public IncidentDetailPage(IncidentDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
