namespace SafeCity.Patterns.Creational.AbstractFactory;

/// <summary>
/// PATTERN: Abstract Factory — Concrete Factory for critical incidents (crime, fire, missing person).
/// </summary>
public class CriticalAlertFactory : IAlertStyleFactory
{
    public AlertStyleBundle CreateBundle() =>
        new("🚨", "#FF3B30", "alert_critical", UseVibration: true, PriorityLevel: 10);

    public string CreateBannerTitle(string incidentTitle) =>
        $"⚠️ ALERT: {incidentTitle}";

    public string CreateBannerBody(string address, double distanceKm) =>
        $"{address} · {distanceKm:F1} km away — Stay alert";
}
