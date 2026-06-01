using SafeCity.ViewModels;

namespace SafeCity.Views;

public partial class AssistantPage : ContentPage
{
    public AssistantPage(AssistantViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
