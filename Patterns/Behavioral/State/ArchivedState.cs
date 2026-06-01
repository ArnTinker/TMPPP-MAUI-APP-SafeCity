using SafeCity.Enums;

namespace SafeCity.Patterns.Behavioral.State;

/// <summary>PATTERN: State — Archived state. Terminal — all transitions blocked.</summary>
public class ArchivedState : IIncidentState
{
    public IncidentStateEnum Value => IncidentStateEnum.Archived;

    public void Verify(IncidentContext ctx)  => Deny();
    public void Resolve(IncidentContext ctx) => Deny();
    public void Archive(IncidentContext ctx) => Deny();
    public void Dispute(IncidentContext ctx) => Deny();

    private static void Deny() =>
        throw new InvalidOperationException("Archived incidents are read-only.");
}
