using SafeCity.Data;
using SafeCity.Models;

namespace SafeCity.Patterns.Behavioral.Command;

/// <summary>PATTERN: Command — Upvote an incident; undo decrements the counter.</summary>
public class UpvoteCommand : IUserCommand
{
    private readonly IIncidentRepository _repo;
    private readonly Incident _incident;

    public string Description => $"Upvote incident #{_incident.Id}";

    public UpvoteCommand(IIncidentRepository repo, Incident incident)
    {
        _repo     = repo;
        _incident = incident;
    }

    public async Task ExecuteAsync()
    {
        _incident.UpvoteCount++;
        await _repo.UpdateAsync(_incident);
    }

    public async Task UndoAsync()
    {
        if (_incident.UpvoteCount > 0)
            _incident.UpvoteCount--;
        await _repo.UpdateAsync(_incident);
    }
}
