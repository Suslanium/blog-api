using System.ComponentModel.DataAnnotations;

namespace blog_api.Model;

public class CommentCreateDto
{
    [MinLength(1), MaxLength(1000)]
    public required string Content { get; set; }
    
    public Guid? ParentCommentId { get; set; }
}