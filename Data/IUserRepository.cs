using SafeCity.Models;

namespace SafeCity.Data;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByUsernameAsync(string username);
    Task<int> InsertAsync(User user);
    Task<int> UpdateAsync(User user);
    Task<bool> IsUsernameAvailableAsync(string username);
}
