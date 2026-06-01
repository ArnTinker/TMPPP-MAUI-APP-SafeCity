using SafeCity.Models;

namespace SafeCity.Services;

public interface IAuthService
{
    Task<User?> LoginAsync(string email, string password);
    Task<User?> RegisterAsync(string username, string email, string password);
    Task LogoutAsync();
    bool IsLoggedIn { get; }
}
