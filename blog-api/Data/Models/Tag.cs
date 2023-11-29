namespace blog_api.Data.Models;

public class Tag
{
    public Guid Id { get; set; }

    public DateTime CreationTime { get; set; }

    public required string Name { get; set; }
    
    public List<Post> Posts { get; } = new();
}