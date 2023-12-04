using System.ComponentModel.DataAnnotations;

namespace blog_api.Model;

public class LoginCredentialsDto
{
    [EmailAddress]
    public required string Email { get; set; }
    
    [MinLength(6)]
    public required string Password { get; set; }
}