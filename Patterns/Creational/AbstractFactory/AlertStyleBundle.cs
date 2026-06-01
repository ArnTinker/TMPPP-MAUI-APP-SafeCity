namespace SafeCity.Patterns.Creational.AbstractFactory;

/// <summary>Product family produced by IAlertStyleFactory.</summary>
public record AlertStyleBundle(
    string IconGlyph,
    string ColorHex,
    string SoundKey,
    bool UseVibration,
    int PriorityLevel
);
