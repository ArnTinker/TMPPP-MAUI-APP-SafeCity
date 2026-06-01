using SafeCity.Models;

namespace SafeCity.Patterns.Behavioral.Observer;

/// <summary>
/// PATTERN: Observer — Observer interface.
/// Justification: Multiple independent ViewModels (map, alerts, news feed) must react
/// to new incidents without being coupled to each other or to the reporting subsystem.
/// </summary>
public interface IIncidentObserver
{
    void OnIncidentPublished(Incident incident);
}
