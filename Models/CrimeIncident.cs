using SafeCity.Enums;

namespace SafeCity.Models;

public class CrimeIncident : Incident
{
    public override string DefaultIcon => "🚨";
    public override string DefaultColor => "#FF3B30";
    public override IncidentSeverity DefaultSeverity => IncidentSeverity.High;
    public override string[] RequiredFields => ["Title", "Description", "Address"];
}
