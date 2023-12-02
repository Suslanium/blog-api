namespace blog_api.Data.Models;

public class Comment
{
    public Guid Id { get; set; }
    
    public required string Content { get; set; }
    
    public required DateTime CreationTime { get; set; }
    
    public DateTime? ModifiedTime { get; set; }
    
    public DateTime? DeletedTime { get; set; }
    
    public required Guid PostId { get; set; }
    
    public Guid? ParentCommentId { get; set; }
    
    public required Guid AuthorId { get; set; }

    public User Author { get; set; } = null!;

    public List<Comment> SubComments { get; } = new();
}