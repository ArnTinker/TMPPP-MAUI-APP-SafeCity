using SafeCity.Data;
using SafeCity.Patterns.Creational.Singleton;

namespace SafeCity.Patterns.Behavioral.Command;

/// <summary>PATTERN: Command — Opt the current user into the Safety Network; undo opts out.</summary>
public class JoinSafetyNetworkCommand : IUserCommand
{
    private readonly SessionManager _session;
    private readonly IUserRepository _users;

    public string Description => "Join Safety Network";

    public JoinSafetyNetworkCommand(SessionManager session, IUserRepository users)
    {
        _session = session;
        _users   = users;
    }

    public async Task ExecuteAsync()
    {
        if (_session.CurrentUser is null) return;
        // Stub: mark the user as premium (Safety Network feature)
        _session.CurrentUser.IsPremium = true;
        await _users.UpdateAsync(_session.CurrentUser);
    }

    public async Task UndoAsync()
    {
        if (_session.CurrentUser is null) return;
        _session.CurrentUser.IsPremium = false;
        await _users.UpdateAsync(_session.CurrentUser);
    }
}
