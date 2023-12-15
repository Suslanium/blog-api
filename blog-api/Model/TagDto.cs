namespace blog_api.Model;

public class TagDto
{
    public required Guid Id { get; set; }
    
    public required DateTime CreationTime { get; set; }
    
    public required string Name { get; set; }
}