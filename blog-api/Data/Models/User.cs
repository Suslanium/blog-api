namespace blog_api.Data.Models;

public class User
{
    public required Guid Id { get; set; }
    
    public required string FullName { get; set; }
    
    public required Gender Gender { get; set; }
    
    public required string PhoneNumber { get; set; }
    
    public required string Email { get; set; }
    
    public required string PasswordHash { get; set; }
    
}