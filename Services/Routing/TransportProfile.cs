namespace SafeCity.Services.Routing;

/// <summary>
/// PATTERN 10 (Strategy — reuse). Each member is a concrete routing strategy.
/// The Mapbox routing/isochrone services select the endpoint algorithm at runtime based
/// on this value, which is a textbook Strategy application: same interface, interchangeable
/// algorithms.
/// </summary>
public enum TransportProfile
{
    Walking,
    Driving,
    Cycling
}

public static class TransportProfileExtensions
{
    /// <summary>Maps each strategy to the Mapbox endpoint profile string.</summary>
    public static string ToMapboxProfile(this TransportProfile profile) => profile switch
    {
        TransportProfile.Walking => "walking",
        TransportProfile.Driving => "driving",
        TransportProfile.Cycling => "cycling",
        _                        => "walking"
    };

    public static string ToDisplayName(this TransportProfile profile) => profile switch
    {
        TransportProfile.Walking => "Walk",
        TransportProfile.Driving => "Drive",
        TransportProfile.Cycling => "Cycle",
        _                        => "Walk"
    };
}
