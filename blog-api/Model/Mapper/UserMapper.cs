﻿using blog_api.Data.Models;

namespace blog_api.Model.Mapper;

public static class UserMapper
{
    public static UserDto GetUserDto(User from)
    {
        return new UserDto
        {
            Id = from.Id,
            Email = from.Email,
            FullName = from.FullName,
            BirthDate = from.BirthDate,
            CreationTime = from.CreationTime,
            Gender = from.Gender,
            PhoneNumber = from.PhoneNumber
        };
    }

    public static User GetUserEntity(UserRegisterDto from)
    {
        return new User
        {
            FullName = from.FullName,
            Email = from.Email,
            Gender = from.Gender,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(from.Password),
            PhoneNumber = from.PhoneNumber,
            CreationTime = DateTime.UtcNow,
            BirthDate = from.BirthDate
        };
    }
}