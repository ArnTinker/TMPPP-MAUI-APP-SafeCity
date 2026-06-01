using SafeCity.Enums;

namespace SafeCity.Patterns.Behavioral.State;

/// <summary>PATTERN: State — Verified state. Allows: Resolve, Dispute. Blocks: Verify again, Archive directly.</summary>
public class VerifiedState : IIncidentState
{
    public IncidentStateEnum Value => IncidentStateEnum.Verified;

    public void Resolve(IncidentContext ctx) => ctx.TransitionTo(new ResolvedState());
    public void Dispute(IncidentContext ctx) => ctx.TransitionTo(new DisputedState());

    public void Verify(IncidentContext ctx) =>
        throw new InvalidOperationException("Incident is already Verified.");

    public void Archive(IncidentContext ctx) =>
        throw new InvalidOperationException("Resolve the incident before archiving.");
}
