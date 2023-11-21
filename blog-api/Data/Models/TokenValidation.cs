using System.ComponentModel.DataAnnotations;

namespace blog_api.Data.Models;

public class TokenValidation
{
    [Key]
    public required string UserEmail { get; set; }
    
    public required DateTime MinimalIssuedTime { get; set; }
}