namespace blog_api.Data.Models;

public class User
{
    public Guid Id { get; set; }
    
    public required string FullName { get; set; }
    
    public required Gender Gender { get; set; }
    
    public required string PhoneNumber { get; set; }
    
    public required DateTime BirthDate { get; set; }
    
    public required DateTime CreationTime { get; set; }
    
    public required string Email { get; set; }
    
    public required string PasswordHash { get; set; }
    
    public List<Community> SubscribedCommunities { get; } = new();

    public List<Subscription> Subscriptions { get; } = new();

    public List<Post> Posts { get; } = new();

    public List<Post> LikedPosts { get; } = new();
}