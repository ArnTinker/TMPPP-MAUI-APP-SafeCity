using SafeCity.Models;

namespace SafeCity.Patterns.Behavioral.Strategy;

/// <summary>PATTERN: Strategy — Sort by upvote + view engagement score.</summary>
public class ByPopularityStrategy : IFeedSortStrategy
{
    public string DisplayName => "Trending";

    public IEnumerable<Incident> Sort(IEnumerable<Incident> incidents, double userLat = 0, double userLon = 0) =>
        incidents.OrderByDescending(i => i.UpvoteCount * 2 + i.ViewCount + i.CommentCount);
}
