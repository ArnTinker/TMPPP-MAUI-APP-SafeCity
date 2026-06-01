using SafeCity.Models;

namespace SafeCity.Patterns.Behavioral.Observer;

/// <summary>
/// PATTERN: Observer — Subject / Publisher.
/// Justification: When ReportingFacade submits a new incident it calls Publish().
/// The map VM, alerts VM, and news VM are all registered observers and update their
/// UI independently without any direct dependency on each other or on the facade.
/// </summary>
public class IncidentAlertPublisher
{
    private readonly List<IIncidentObserver> _observers = new();

    public void Subscribe(IIncidentObserver observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    public void Unsubscribe(IIncidentObserver observer) =>
        _observers.Remove(observer);

    public void Publish(Incident incident)
    {
        foreach (var obs in _observers.ToList())
            obs.OnIncidentPublished(incident);
    }
}
