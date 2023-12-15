namespace blog_api.Model;

public class CommentDto
{
    public required Guid Id { get; set; }
    
    public Guid? ParentCommentId { get; set; }
    
    public required DateTime CreationTime { get; set; }
    
    public required string Content { get; set; }
    
    public DateTime? ModifiedTime { get; set; }
    
    public DateTime? DeleteTime { get; set; }
    
    public required Guid AuthorId { get; set; }
    
    public required string AuthorName { get; set; }
    
    public required int SubCommentsCount { get; set; }
}