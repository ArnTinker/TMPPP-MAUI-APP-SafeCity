using SafeCity.Enums;

namespace SafeCity.Patterns.Behavioral.State;

/// <summary>PATTERN: State — Reported state. Allows: Verify, Dispute. Blocks: Resolve, Archive.</summary>
public class ReportedState : IIncidentState
{
    public IncidentStateEnum Value => IncidentStateEnum.Reported;

    public void Verify(IncidentContext ctx)  => ctx.TransitionTo(new VerifiedState());
    public void Dispute(IncidentContext ctx) => ctx.TransitionTo(new DisputedState());

    public void Resolve(IncidentContext ctx) =>
        throw new InvalidOperationException("An incident must be Verified before it can be Resolved.");

    public void Archive(IncidentContext ctx) =>
        throw new InvalidOperationException("Archive is only allowed from Resolved state.");
}
