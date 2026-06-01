using SafeCity.Services;

namespace SafeCity.Patterns.Structural.Decorator;

/// <summary>
/// PATTERN: Decorator — Concrete Decorator (sound).
/// Justification: Adds a sound cue to any notification without changing the wrapped
/// object. Stack with other decorators to combine behaviours at runtime.
/// </summary>
public class SoundNotificationDecorator : NotificationDecorator
{
    private readonly string _soundKey;

    public SoundNotificationDecorator(IAppNotification inner, string soundKey = "alert_default")
        : base(inner) => _soundKey = soundKey;

    public override void Show()
    {
        // Sound playback stub — replace with MediaElement or platform audio API
        System.Diagnostics.Debug.WriteLine($"[Sound] Playing {_soundKey}");
        base.Show();
    }
}
