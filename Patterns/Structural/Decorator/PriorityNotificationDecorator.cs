using SafeCity.Services;

namespace SafeCity.Patterns.Structural.Decorator;

/// <summary>
/// PATTERN: Decorator — Concrete Decorator (priority badge).
/// Justification: Prepends "🔴 HIGH PRIORITY" to the notification title without
/// modifying the underlying notification object or creating a subclass explosion.
/// </summary>
public class PriorityNotificationDecorator : NotificationDecorator
{
    public PriorityNotificationDecorator(IAppNotification inner) : base(inner) { }

    public override string Title => $"🔴 {_inner.Title}";

    public override void Show()
    {
        try
        {
            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(200));
        }
        catch { /* vibrator unavailable — notification still shows */ }
        base.Show();
    }
}
