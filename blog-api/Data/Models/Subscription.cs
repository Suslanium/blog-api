namespace blog_api.Data.Models;

public class Subscription
{
    public Guid UserId { get; set; }
    public Guid CommunityId { get; set; }
    public CommunityRole CommunityRole { get; set; }
    public User User { get; set; } = null!;
    public Community Community { get; set; } = null!;
}