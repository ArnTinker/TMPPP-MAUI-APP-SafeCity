using SafeCity.Enums;

namespace SafeCity.Models;

public class AccidentIncident : Incident
{
    public override string DefaultIcon => "💥";
    public override string DefaultColor => "#FF9500";
    public override IncidentSeverity DefaultSeverity => IncidentSeverity.High;
    public override string[] RequiredFields => ["Title", "Description"];
}
