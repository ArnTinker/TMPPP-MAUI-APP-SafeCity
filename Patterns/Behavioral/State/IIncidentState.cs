using SafeCity.Enums;

namespace SafeCity.Patterns.Behavioral.State;

/// <summary>
/// PATTERN: State — State interface.
/// Justification: An incident's valid actions depend on its current lifecycle position.
/// Placing allowed-transition logic inside state objects (instead of a giant switch)
/// makes each state self-documenting and easy to extend (OCP).
/// </summary>
public interface IIncidentState
{
    IncidentStateEnum Value { get; }
    void Verify(IncidentContext ctx);
    void Resolve(IncidentContext ctx);
    void Archive(IncidentContext ctx);
    void Dispute(IncidentContext ctx);
}
