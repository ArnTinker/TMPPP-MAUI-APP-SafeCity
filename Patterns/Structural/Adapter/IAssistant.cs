namespace SafeCity.Patterns.Structural.Adapter;

/// <summary>App-internal AI assistant contract — decoupled from any provider.</summary>
public interface IAssistant
{
    Task<string> AskAsync(string prompt, CancellationToken ct = default);
    bool IsAvailable { get; }
}
