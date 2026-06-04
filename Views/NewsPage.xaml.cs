using SafeCity.Models;
using SafeCity.ViewModels;

namespace SafeCity.Views;

public partial class NewsPage : ContentPage
{
    private readonly NewsViewModel _vm;

    public NewsPage(NewsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!_vm.IsBusy)
            _vm.LoadCommand.Execute(null);
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Incident incident) return;
        FeedList.SelectedItem = null; // deselect so the card doesn't stay highlighted
        await Shell.Current.GoToAsync($"IncidentDetailPage?id={incident.Id}");
    }
}
