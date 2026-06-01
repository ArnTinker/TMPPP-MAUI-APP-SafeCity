using SafeCity.Services;

namespace SafeCity.Patterns.Structural.Decorator;

/// <summary>
/// PATTERN: Decorator — Concrete Component (base notification with no extra behaviours).
/// Justification: This is the plain implementation that decorators wrap. Kept separate
/// so the decorator chain can always terminate at a well-defined base.
/// </summary>
public class BaseNotification : IAppNotification
{
    public string Title { get; }
    public string Body { get; }

    public BaseNotification(string title, string body)
    {
        Title = title;
        Body  = body;
    }

    public void Show()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is not null)
                await page.DisplayAlertAsync(Title, Body, "OK");
        });
    }
}
