using SafeCity.Enums;
using SafeCity.Models;

namespace SafeCity.Patterns.Creational.Builder;

/// <summary>
/// PATTERN: Builder — Builder interface.
/// Justification: A Report is assembled across multiple UI steps (location, category,
/// severity, description, media, anonymity). The builder enforces step-by-step
/// construction with validation at Build(), preventing half-initialised objects.
/// </summary>
public interface IReportBuilder
{
    IReportBuilder WithLocation(double lat, double lon, string address);
    IReportBuilder WithCategory(IncidentType type);
    IReportBuilder WithSeverity(IncidentSeverity severity);
    IReportBuilder WithDescription(string description);
    IReportBuilder WithMedia(IEnumerable<string> mediaPaths);
    IReportBuilder WithAnonymity(bool anonymous);
    IReportBuilder WithReporter(string reporterId);
    Report Build();
    void Reset();
}
