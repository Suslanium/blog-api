using System.ComponentModel.DataAnnotations;

namespace blog_api.Model;

public class TagCreationDto
{
    [MinLength(1)]
    public required string Name { get; set; }
}