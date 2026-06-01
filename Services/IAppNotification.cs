namespace SafeCity.Services;

/// <summary>
/// Core notification contract — also serves as the component interface for the Decorator chain.
/// </summary>
public interface IAppNotification
{
    string Title { get; }
    string Body { get; }
    void Show();
}
