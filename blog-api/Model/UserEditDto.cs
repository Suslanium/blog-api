﻿using System.ComponentModel.DataAnnotations;
using blog_api.Data.Models;

namespace blog_api.Model;

public class UserEditDto
{
    [MinLength(2)]
    public required string FullName { get; set; }
    
    public required Gender Gender { get; set; }
    
    public DateTime? BirthDate { get; set; }
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    [EmailAddress]
    public required string Email { get; set; }
}