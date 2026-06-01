using System.Text.Json;
using SafeCity.Enums;
using SafeCity.Models;

namespace SafeCity.Patterns.Creational.Builder;

/// <summary>
/// PATTERN: Builder — Concrete Builder.
/// Justification: Assembles a Report step by step with a fluent API. Build() validates
/// that all required fields are present before returning the complete object.
/// </summary>
public class ReportBuilder : IReportBuilder
{
    private Report _report = new();

    public IReportBuilder WithLocation(double lat, double lon, string address)
    {
        _report.Latitude  = lat;
        _report.Longitude = lon;
        _report.Address   = address;
        return this;
    }

    public IReportBuilder WithCategory(IncidentType type)
    {
        _report.Category = type;
        return this;
    }

    public IReportBuilder WithSeverity(IncidentSeverity severity)
    {
        _report.Severity = severity;
        return this;
    }

    public IReportBuilder WithDescription(string description)
    {
        _report.Description = description;
        return this;
    }

    public IReportBuilder WithMedia(IEnumerable<string> mediaPaths)
    {
        _report.MediaUrlsJson = JsonSerializer.Serialize(mediaPaths.ToList());
        return this;
    }

    public IReportBuilder WithAnonymity(bool anonymous)
    {
        _report.IsAnonymous = anonymous;
        return this;
    }

    public IReportBuilder WithReporter(string reporterId)
    {
        _report.ReporterId = reporterId;
        return this;
    }

    public Report Build()
    {
        if (_report.Latitude == 0 && _report.Longitude == 0)
            throw new InvalidOperationException("Location is required before building a report.");
        if (string.IsNullOrWhiteSpace(_report.Description) &&
            _report.Category != IncidentType.GoodVibes)
            throw new InvalidOperationException("Description is required.");

        _report.SubmittedAt = DateTime.UtcNow;
        var built = _report;
        Reset();
        return built;
    }

    public void Reset() => _report = new Report();
}
