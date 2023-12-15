using blog_api.Data.Models;

namespace blog_api.Model;

public class AuthorDto
{
    public required string FullName { get; set; }
    
    public DateTime? BirthDate { get; set; }
    
    public required Gender Gender { get; set; }
    
    public required int Posts { get; set; }
    
    public required int Likes { get; set; }

    public required DateTime CreationTime { get; set; }
}