using SafeCity.Models;

namespace SafeCity.Patterns.Behavioral.Command;

/// <summary>PATTERN: Command — Share an incident via the platform share sheet; undo is a no-op.</summary>
public class ShareIncidentCommand : IUserCommand
{
    private readonly Incident _incident;

    public string Description => $"Share incident #{_incident.Id}";

    public ShareIncidentCommand(Incident incident) => _incident = incident;

    public async Task ExecuteAsync()
    {
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title   = _incident.Title,
            Text    = $"SafeCity Alert: {_incident.Title}\n{_incident.Address}\nvia SafeCity app",
            Subject = _incident.Title
        });
    }

    public Task UndoAsync() => Task.CompletedTask; // sharing cannot be undone
}
