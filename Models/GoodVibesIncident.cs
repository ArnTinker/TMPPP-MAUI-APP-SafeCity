using SafeCity.Enums;

namespace SafeCity.Models;

public class GoodVibesIncident : Incident
{
    public override string DefaultIcon => "✅";
    public override string DefaultColor => "#34C759";
    public override IncidentSeverity DefaultSeverity => IncidentSeverity.Low;
    public override string[] RequiredFields => ["Title"];
}
