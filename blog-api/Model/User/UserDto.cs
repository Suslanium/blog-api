using System.ComponentModel.DataAnnotations;
using blog_api.Data.Models;

namespace blog_api.Model;

public class UserDto
{
    public required Guid Id { get; set; }
    
    public required DateTime CreationTime { get; set; }
    
    public required string FullName { get; set; }
    
    public DateTime? BirthDate { get; set; }
    
    public required Gender Gender { get; set; }
    
    [EmailAddress]
    public required string Email { get; set; }
    
    [Phone]
    public string? PhoneNumber { get; set; }
}