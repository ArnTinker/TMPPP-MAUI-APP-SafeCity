using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SafeCity.Patterns.Structural.Adapter;

namespace SafeCity.ViewModels;

public record ChatMessage(string Sender, string Text, bool IsUser);

public partial class AssistantViewModel : BaseViewModel
{
    private readonly IAssistant _assistant;

    [ObservableProperty] private ObservableCollection<ChatMessage> _messages = [];
    [ObservableProperty] private string  _inputText    = string.Empty;
    [ObservableProperty] private bool    _isAvailable;

    public AssistantViewModel(IAssistant assistant)
    {
        _assistant   = assistant;
        _isAvailable = assistant.IsAvailable;
        Title = "SafeCity Assistant";

        Messages.Add(new ChatMessage("SafeCity AI",
            IsAvailable
                ? "Hi! I'm your SafeCity safety assistant. Ask me about staying safe, understanding alerts, or local emergency guidance."
                : "Assistant not configured. Add GEMINI_API_KEY to .env to enable.",
            IsUser: false));
    }

    [RelayCommand]
    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText)) return;
        var userMsg = InputText;
        InputText = string.Empty;
        Messages.Add(new ChatMessage("You", userMsg, IsUser: true));
        IsBusy = true;
        var reply = await _assistant.AskAsync(userMsg);
        Messages.Add(new ChatMessage("SafeCity AI", reply, IsUser: false));
        IsBusy = false;
    }
}
