using SQLite;

namespace SafeCity.Models;

[Table("Users")]
public class User
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsPremium { get; set; }
    public int ReportCount { get; set; }
    public int UpvotesReceived { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public string? AuthToken { get; set; }
}
