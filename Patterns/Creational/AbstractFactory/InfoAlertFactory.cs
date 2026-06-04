namespace SafeCity.Patterns.Creational.AbstractFactory;

/// <summary>
/// PATTERN: Abstract Factory — Concrete Factory for informational incidents (road hazard, good vibes).
/// </summary>
public class InfoAlertFactory : IAlertStyleFactory
{
    public AlertStyleBundle CreateBundle() =>
        new("ℹ️", "#9A9A9A", "alert_info", UseVibration: false, PriorityLevel: 3);

    public string CreateBannerTitle(string incidentTitle) =>
        $"SafeCity · {incidentTitle}";

    public string CreateBannerBody(string address, double distanceKm) =>
        $"{address} · {distanceKm:F1} km away";
}
