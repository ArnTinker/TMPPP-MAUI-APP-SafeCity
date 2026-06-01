using SafeCity.Data;
using SafeCity.Enums;
using SafeCity.Models;
using SafeCity.Patterns.Behavioral.Observer;
using SafeCity.Patterns.Creational.Builder;
using SafeCity.Patterns.Creational.FactoryMethod;
using SafeCity.Patterns.Creational.Singleton;

namespace SafeCity.Patterns.Structural.Facade;

/// <summary>
/// PATTERN: Facade — Concrete Facade.
/// Justification: SubmitAsync is the only method the ViewModel calls. Internally it
/// coordinates ReportBuilder → ReportRepository → IncidentFactory → IncidentRepository
/// → IncidentAlertPublisher without exposing those collaborators to the caller.
/// </summary>
public class ReportingFacade : IReportingFacade
{
    private readonly IReportBuilder _builder;
    private readonly IReportRepository _reportRepo;
    private readonly IIncidentRepository _incidentRepo;
    private readonly IIncidentCreator _factory;
    private readonly IncidentAlertPublisher _publisher;
    private readonly SessionManager _session;

    public ReportingFacade(
        IReportBuilder builder,
        IReportRepository reportRepo,
        IIncidentRepository incidentRepo,
        IIncidentCreator factory,
        IncidentAlertPublisher publisher,
        SessionManager session)
    {
        _builder     = builder;
        _reportRepo  = reportRepo;
        _incidentRepo = incidentRepo;
        _factory     = factory;
        _publisher   = publisher;
        _session     = session;
    }

    public async Task<Incident> SubmitAsync(
        double lat, double lon, string address,
        IncidentType type, IncidentSeverity severity,
        string description, IEnumerable<string> mediaPaths,
        bool anonymous)
    {
        // Step 1 — build the Report via the Builder
        var report = _builder
            .WithLocation(lat, lon, address)
            .WithCategory(type)
            .WithSeverity(severity)
            .WithDescription(description)
            .WithMedia(mediaPaths)
            .WithAnonymity(anonymous)
            .WithReporter(_session.CurrentUser?.Id ?? "guest")
            .Build();

        await _reportRepo.InsertAsync(report);

        // Step 2 — create the corresponding Incident via the Factory
        var incident = _factory.Create(type);
        incident.Title       = description.Length > 60 ? description[..60] + "…" : description;
        incident.Description = description;
        incident.Latitude    = lat;
        incident.Longitude   = lon;
        incident.Address     = address;
        incident.Severity    = severity;
        incident.IsAnonymous = anonymous;
        incident.ReporterId  = report.ReporterId;
        incident.MediaUrlsJson = report.MediaUrlsJson;

        await _incidentRepo.InsertAsync(incident);

        // Step 3 — notify all observers (map VM, alerts VM, feed VM)
        _publisher.Publish(incident);

        return incident;
    }
}
