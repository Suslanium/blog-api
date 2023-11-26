using System.ComponentModel.DataAnnotations;

namespace blog_api.Model;

public class CommunityCreateEditDto
{
    [MinLength(1)]
    public required string Name { get; set; }
    
    public required string Description { get; set; }
    
    public required bool IsClosed { get; set; }
}