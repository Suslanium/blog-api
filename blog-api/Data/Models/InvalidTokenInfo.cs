namespace blog_api.Data.Models;

public class InvalidTokenInfo
{
    public required Guid UserId { get; set; }
    
    public required DateTime IssuedTime { get; set; }
}