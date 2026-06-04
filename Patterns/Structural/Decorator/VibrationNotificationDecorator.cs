using SafeCity.Services;

namespace SafeCity.Patterns.Structural.Decorator;

/// <summary>
/// PATTERN: Decorator — Concrete Decorator (vibration).
/// Justification: Adds haptic feedback to any notification dynamically. Combined with
/// PriorityDecorator and SoundDecorator via constructor chaining in the notification
/// factory, producing bespoke behaviour for each severity level.
/// </summary>
public class VibrationNotificationDecorator : NotificationDecorator
{
    private readonly int _durationMs;

    public VibrationNotificationDecorator(IAppNotification inner, int durationMs = 500)
        : base(inner) => _durationMs = durationMs;

    public override void Show()
    {
        try
        {
            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(_durationMs));
        }
        catch { /* vibrator unavailable — notification still shows */ }
        base.Show();
    }
}
