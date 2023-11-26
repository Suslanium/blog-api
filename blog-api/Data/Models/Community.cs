namespace blog_api.Data.Models;

public class Community
{
    public Guid Id { get; set; }
    
    public required string Name { get; set; }
    
    public required string Description { get; set; }
    
    public required bool IsClosed { get; set; }
    
    public required DateTime CreationTime { get; set; }

    public List<User> Subscribers { get; } = new();

    public List<Subscription> Subscriptions { get; } = new();

    public List<Post> Posts { get; } = new();
}