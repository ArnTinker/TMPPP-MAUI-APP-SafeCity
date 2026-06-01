using SafeCity.Enums;
using SafeCity.Models;

namespace SafeCity.Patterns.Behavioral.State;

/// <summary>
/// PATTERN: State — Context.
/// Holds the current IIncidentState and delegates all lifecycle calls to it.
/// Also keeps the persisted Incident.State enum in sync.
/// </summary>
public class IncidentContext
{
    private IIncidentState _state;
    public Incident Incident { get; }

    public IncidentContext(Incident incident)
    {
        Incident = incident;
        _state   = StateFromEnum(incident.State);
    }

    public IncidentStateEnum CurrentState => _state.Value;

    internal void TransitionTo(IIncidentState newState)
    {
        _state          = newState;
        Incident.State  = newState.Value;
        Incident.UpdatedAt = DateTime.UtcNow;
    }

    public void Verify()  => _state.Verify(this);
    public void Resolve() => _state.Resolve(this);
    public void Archive() => _state.Archive(this);
    public void Dispute() => _state.Dispute(this);

    private static IIncidentState StateFromEnum(IncidentStateEnum e) => e switch
    {
        IncidentStateEnum.Reported => new ReportedState(),
        IncidentStateEnum.Verified => new VerifiedState(),
        IncidentStateEnum.Resolved => new ResolvedState(),
        IncidentStateEnum.Archived => new ArchivedState(),
        IncidentStateEnum.Disputed => new DisputedState(),
        _                          => new ReportedState()
    };
}
