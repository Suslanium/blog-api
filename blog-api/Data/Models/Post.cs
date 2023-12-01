namespace blog_api.Data.Models;

public class Post
{
    public Guid Id { get; set; }
    
    public required DateTime CreationTime { get; set; }
    
    public DateTime? EditedTime { get; set; }
    
    public required string Title { get; set; }
    
    public required string Description { get; set; }
    
    public required int ReadingTime { get; set; }
    
    public string? ImageUri { get; set; }
    
    public Guid? AddressId { get; set; }
    
    public required Guid AuthorId { get; set; }

    public User Author { get; set; } = null!;
    
    public Guid? CommunityId { get; set; }

    public Community? Community { get; set; } = null!;
    
    public List<Tag> Tags { get; } = new();
    
    public int LikeCount { get; set; }

    public List<LikedPosts> Likes { get; } = new();
}