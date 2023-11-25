
using System.ComponentModel.DataAnnotations;

namespace blog_api.Data.Models;

public class TokenValidation
{
    [Key]
    public required Guid UserId { get; set; }
    
    public required DateTime MinimalIssuedTime { get; set; }
}