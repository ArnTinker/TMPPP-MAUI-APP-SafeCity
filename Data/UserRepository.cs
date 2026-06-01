using SafeCity.Models;

namespace SafeCity.Data;

public class UserRepository : IUserRepository
{
    private readonly DatabaseContext _ctx;

    public UserRepository(DatabaseContext ctx) => _ctx = ctx;

    public async Task<User?> GetByIdAsync(string id)
    {
        var db = await _ctx.GetConnectionAsync();
        return await db.FindAsync<User>(id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var db = await _ctx.GetConnectionAsync();
        return await db.Table<User>().Where(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<int> InsertAsync(User user)
    {
        var db = await _ctx.GetConnectionAsync();
        return await db.InsertAsync(user);
    }

    public async Task<int> UpdateAsync(User user)
    {
        var db = await _ctx.GetConnectionAsync();
        return await db.UpdateAsync(user);
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        var user = await GetByUsernameAsync(username);
        return user is null;
    }
}
