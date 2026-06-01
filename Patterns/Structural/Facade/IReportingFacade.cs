using SafeCity.Enums;
using SafeCity.Models;

namespace SafeCity.Patterns.Structural.Facade;

/// <summary>
/// PATTERN: Facade — Facade interface.
/// Justification: Submitting a report requires coordinating six subsystems
/// (location, builder, media, repository, observer, backend API). The facade
/// exposes a single SubmitAsync method so ViewModels have no knowledge of
/// subsystem internals (Single Responsibility + Dependency Inversion).
/// </summary>
public interface IReportingFacade
{
    Task<Incident> SubmitAsync(
        double lat, double lon, string address,
        IncidentType type, IncidentSeverity severity,
        string description, IEnumerable<string> mediaPaths,
        bool anonymous);
}
