namespace SafeCity.Services.Routing;

/// <summary>
/// PATTERN 10 (Strategy — reuse). Each member is a concrete routing strategy.
/// OrsRoutingService selects the ORS endpoint algorithm at runtime based on this value,
/// which is a textbook Strategy application: same interface, interchangeable algorithms.
/// </summary>
public enum TransportProfile
{
    Walking,
    Driving,
    Cycling
}

public static class TransportProfileExtensions
{
    /// <summary>Maps each strategy to the ORS endpoint profile string.</summary>
    public static string ToOrsProfile(this TransportProfile profile) => profile switch
    {
        TransportProfile.Walking => "foot-walking",
        TransportProfile.Driving => "driving-car",
        TransportProfile.Cycling => "cycling-regular",
        _                        => "foot-walking"
    };

    public static string ToDisplayName(this TransportProfile profile) => profile switch
    {
        TransportProfile.Walking => "Walk",
        TransportProfile.Driving => "Drive",
        TransportProfile.Cycling => "Cycle",
        _                        => "Walk"
    };
}
