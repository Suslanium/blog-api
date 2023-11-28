using System.ComponentModel.DataAnnotations;

namespace blog_api.Model;

public class CreatePostDto
{
    [MinLength(5)]
    public required string Title { get; set; }
    
    [MinLength(5)]
    public required string Description { get; set; }
    
    [Range(0, int.MaxValue)]
    public required int ReadingTime { get; set; }
    
    [Url]
    public string? ImageUri { get; set; }
    
    public Guid? AddressId { get; set; }
    
    [MinLength(1)]
    public required List<Guid> Tags { get; set; }
}