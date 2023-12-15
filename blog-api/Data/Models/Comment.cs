namespace blog_api.Data.Models;

public class Comment
{
    public Guid Id { get; set; }
    
    public required string Content { get; set; }
    
    public required DateTime CreationTime { get; set; }
    
    public DateTime? ModifiedTime { get; set; }
    
    public DateTime? DeletedTime { get; set; }
    
    public required Guid PostId { get; set; }

    public Post Post { get; set; } = null!;
    
    public Guid? ParentCommentId { get; set; }
    
    public Comment? ParentComment { get; set; }
    
    public Guid? TopLevelParentCommentId { get; set; }
    
    public required Guid AuthorId { get; set; }

    public User Author { get; set; } = null!;
    
    public int SubCommentCount { get; set; }

    public List<Comment> SubComments { get; } = new();
}