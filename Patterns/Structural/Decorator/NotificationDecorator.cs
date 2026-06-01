using SafeCity.Services;

namespace SafeCity.Patterns.Structural.Decorator;

/// <summary>
/// PATTERN: Decorator — Abstract Decorator.
/// Justification: Holds a reference to the wrapped IAppNotification and delegates Show()
/// to it. Concrete decorators override Show() to add behaviour before/after the delegation.
/// </summary>
public abstract class NotificationDecorator : IAppNotification
{
    protected readonly IAppNotification _inner;

    protected NotificationDecorator(IAppNotification inner) => _inner = inner;

    public virtual string Title => _inner.Title;
    public virtual string Body  => _inner.Body;
    public virtual void Show()  => _inner.Show();
}
