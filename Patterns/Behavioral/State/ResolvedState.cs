using SafeCity.Enums;

namespace SafeCity.Patterns.Behavioral.State;

/// <summary>PATTERN: State — Resolved state. Allows: Archive. Blocks all other transitions.</summary>
public class ResolvedState : IIncidentState
{
    public IncidentStateEnum Value => IncidentStateEnum.Resolved;

    public void Archive(IncidentContext ctx) => ctx.TransitionTo(new ArchivedState());

    public void Verify(IncidentContext ctx) =>
        throw new InvalidOperationException("Cannot re-verify a resolved incident.");

    public void Resolve(IncidentContext ctx) =>
        throw new InvalidOperationException("Incident is already Resolved.");

    public void Dispute(IncidentContext ctx) =>
        throw new InvalidOperationException("Resolved incidents cannot be disputed.");
}
