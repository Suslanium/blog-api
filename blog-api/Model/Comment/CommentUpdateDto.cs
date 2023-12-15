using System.ComponentModel.DataAnnotations;

namespace blog_api.Model;

public class CommentUpdateDto
{
    [MinLength(1), MaxLength(1000)]
    public required string Content { get; set; }
}