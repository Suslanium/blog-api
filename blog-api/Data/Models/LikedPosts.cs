namespace blog_api.Data.Models;

public class LikedPosts
{
    public Guid UserId { get; set; }
    
    public Guid PostId { get; set; }
}