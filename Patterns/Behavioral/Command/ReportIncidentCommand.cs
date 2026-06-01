using SafeCity.Data;
using SafeCity.Enums;
using SafeCity.Models;
using SafeCity.Patterns.Structural.Facade;

namespace SafeCity.Patterns.Behavioral.Command;

/// <summary>PATTERN: Command — Encapsulates the full report-submission action with undo (soft-delete).</summary>
public class ReportIncidentCommand : IUserCommand
{
    private readonly IReportingFacade _facade;
    private readonly IIncidentRepository _repo;
    private readonly double _lat, _lon;
    private readonly string _address, _description;
    private readonly IncidentType _type;
    private readonly IncidentSeverity _severity;
    private readonly bool _anonymous;
    private Incident? _submitted;

    public string Description => $"Report {_type} at {_address}";

    public ReportIncidentCommand(
        IReportingFacade facade, IIncidentRepository repo,
        double lat, double lon, string address,
        IncidentType type, IncidentSeverity severity,
        string description, bool anonymous)
    {
        _facade = facade; _repo = repo;
        _lat = lat; _lon = lon; _address = address;
        _type = type; _severity = severity;
        _description = description; _anonymous = anonymous;
    }

    public async Task ExecuteAsync()
    {
        _submitted = await _facade.SubmitAsync(
            _lat, _lon, _address, _type, _severity, _description, [], _anonymous);
    }

    public async Task UndoAsync()
    {
        if (_submitted is not null)
            await _repo.DeleteAsync(_submitted.Id);
    }
}
