using System.ComponentModel.DataAnnotations;
using blog_api.Data.Models;

namespace blog_api.Model;

public class UserEditDto
{
    [MinLength(2)] public required string FullName { get; set; }

    public required Gender Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    [RegularExpression(@"^\+7\s\(\d{3}\)\s\d{3}-\d{2}-\d{2}$",
        ErrorMessage = "Phone number must be in the format +7 (xxx) xxx-xx-xx, where x is a digit")]
    public string? PhoneNumber { get; set; }

    [EmailAddress] public required string Email { get; set; }
}