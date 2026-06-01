using SafeCity.Data;
using SafeCity.Models;
using SafeCity.Patterns.Creational.Singleton;

namespace SafeCity.Services;

/// <summary>Stubbed local auth — swap for real OAuth in production.</summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly SessionManager _session;

    public AuthService(IUserRepository users, SessionManager session)
    {
        _users = users;
        _session = session;
    }

    public bool IsLoggedIn => _session.CurrentUser is not null;

    public async Task<User?> LoginAsync(string email, string password)
    {
        var user = await _users.GetByUsernameAsync(email)
                   ?? await _users.GetByUsernameAsync(email.Split('@')[0]);
        if (user is null) return null;
        _session.CurrentUser = user;
        _session.AuthToken = $"stub-token-{user.Id}";
        return user;
    }

    public async Task<User?> RegisterAsync(string username, string email, string password)
    {
        var user = new User { Username = username, Email = email };
        await _users.InsertAsync(user);
        _session.CurrentUser = user;
        _session.AuthToken = $"stub-token-{user.Id}";
        return user;
    }

    public Task LogoutAsync()
    {
        _session.CurrentUser = null;
        _session.AuthToken = null;
        return Task.CompletedTask;
    }
}
