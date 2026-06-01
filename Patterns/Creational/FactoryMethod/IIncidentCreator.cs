using SafeCity.Enums;
using SafeCity.Models;

namespace SafeCity.Patterns.Creational.FactoryMethod;

/// <summary>
/// PATTERN: Factory Method — Creator interface.
/// Justification: Decouples callers from concrete Incident subtypes.
/// New types (e.g. NaturalDisaster) require only a new subclass + a factory case,
/// with zero changes to ViewModels or services (Open/Closed Principle).
/// </summary>
public interface IIncidentCreator
{
    Incident Create(IncidentType type);
    Incident Hydrate(Incident stored);
}
