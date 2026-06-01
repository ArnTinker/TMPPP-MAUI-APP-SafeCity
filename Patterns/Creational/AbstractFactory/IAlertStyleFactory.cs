namespace SafeCity.Patterns.Creational.AbstractFactory;

/// <summary>
/// PATTERN: Abstract Factory — Abstract Factory interface.
/// Justification: Alert presentation requires a consistent family of objects
/// (icon + color + sound + vibration). Using a factory per category (Critical, Info)
/// ensures all related choices are always coherent; swapping a category only requires
/// swapping the factory, not editing each property individually.
/// </summary>
public interface IAlertStyleFactory
{
    AlertStyleBundle CreateBundle();
    string CreateBannerTitle(string incidentTitle);
    string CreateBannerBody(string address, double distanceKm);
}
