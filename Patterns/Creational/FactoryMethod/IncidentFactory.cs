using SafeCity.Enums;
using SafeCity.Models;

namespace SafeCity.Patterns.Creational.FactoryMethod;

/// <summary>
/// PATTERN: Factory Method — Concrete Creator.
/// Justification: Each IncidentType maps to a distinct subclass with its own default
/// icon, color, severity, and required fields. The factory encapsulates that mapping
/// so callers never switch on IncidentType directly.
/// </summary>
public class IncidentFactory : IIncidentCreator
{
    public Incident Create(IncidentType type) => type switch
    {
        IncidentType.Crime              => new CrimeIncident          { Type = type, Severity = new CrimeIncident().DefaultSeverity },
        IncidentType.Fire               => new FireIncident           { Type = type, Severity = new FireIncident().DefaultSeverity },
        IncidentType.Accident           => new AccidentIncident       { Type = type, Severity = new AccidentIncident().DefaultSeverity },
        IncidentType.RoadHazard         => new HazardIncident         { Type = type, Severity = new HazardIncident().DefaultSeverity },
        IncidentType.SuspiciousActivity => new CrimeIncident          { Type = type, Severity = IncidentSeverity.Medium },
        IncidentType.MissingPerson      => new MissingPersonIncident  { Type = type, Severity = new MissingPersonIncident().DefaultSeverity },
        IncidentType.GoodVibes          => new GoodVibesIncident      { Type = type, Severity = new GoodVibesIncident().DefaultSeverity },
        _                               => new Incident               { Type = type }
    };

    /// <summary>
    /// Re-wraps a plain Incident loaded from the database in the correct subtype
    /// so callers get the right DefaultIcon/DefaultColor without re-querying.
    /// </summary>
    public Incident Hydrate(Incident stored)
    {
        var typed = Create(stored.Type);
        typed.Id             = stored.Id;
        typed.Title          = stored.Title;
        typed.Description    = stored.Description;
        typed.Latitude       = stored.Latitude;
        typed.Longitude      = stored.Longitude;
        typed.Address        = stored.Address;
        typed.CreatedAt      = stored.CreatedAt;
        typed.UpdatedAt      = stored.UpdatedAt;
        typed.Severity       = stored.Severity;
        typed.State          = stored.State;
        typed.UpvoteCount    = stored.UpvoteCount;
        typed.ViewCount      = stored.ViewCount;
        typed.CommentCount   = stored.CommentCount;
        typed.IsAnonymous    = stored.IsAnonymous;
        typed.ReporterId     = stored.ReporterId;
        typed.MediaUrlsJson  = stored.MediaUrlsJson;
        return typed;
    }
}
