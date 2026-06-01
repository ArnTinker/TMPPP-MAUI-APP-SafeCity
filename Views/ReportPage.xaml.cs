using SafeCity.ViewModels;

namespace SafeCity.Views;

public partial class ReportPage : ContentPage
{
    public ReportPage(ReportViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
