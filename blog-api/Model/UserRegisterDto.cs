using System.ComponentModel.DataAnnotations;
using blog_api.Data.Models;

namespace blog_api.Model;

public class UserRegisterDto
{
    [MinLength(2)]
    public required string FullName { get; set; }
    
    public required Gender Gender { get; set; }
    
    public required DateTime BirthDate { get; set; }
    
    [Phone]
    public required string PhoneNumber { get; set; }
    
    [EmailAddress]
    public required string Email { get; set; }
    
    [MinLength(6)]
    public required string Password { get; set; }
    
}