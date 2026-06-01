using SafeCity.Enums;

namespace SafeCity.Patterns.Behavioral.State;

/// <summary>PATTERN: State — Disputed state. Allows re-verification. Blocks resolve and archive.</summary>
public class DisputedState : IIncidentState
{
    public IncidentStateEnum Value => IncidentStateEnum.Disputed;

    public void Verify(IncidentContext ctx) => ctx.TransitionTo(new VerifiedState());

    public void Resolve(IncidentContext ctx) =>
        throw new InvalidOperationException("Resolve the dispute (re-verify) before resolving.");

    public void Archive(IncidentContext ctx) =>
        throw new InvalidOperationException("Disputed incidents cannot be archived.");

    public void Dispute(IncidentContext ctx) =>
        throw new InvalidOperationException("Incident is already in Disputed state.");
}
