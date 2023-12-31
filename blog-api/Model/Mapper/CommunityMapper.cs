﻿using blog_api.Data.Models;

namespace blog_api.Model.Mapper;

public static class CommunityMapper
{
    public static CommunityDto GetCommunityDto(Community from)
        => new CommunityDto
        {
            Id = from.Id,
            CreationTime = from.CreationTime,
            Name = from.Name,
            Description = from.Description,
            IsClosed = from.IsClosed,
            SubscribersCount = from.SubscribersCount
        };

    public static CommunityUserDto GetCommunityUserDto(Community from, Subscription userSub)
        => new CommunityUserDto
        {
            Id = from.Id,
            CreationTime = from.CreationTime,
            Name = from.Name,
            Description = from.Description,
            IsClosed = from.IsClosed,
            SubscribersCount = from.SubscribersCount,
            UserRole = userSub.CommunityRole
        };

    public static CommunityFullDto GetFullCommunityDto(Community from)
        => new CommunityFullDto
        {
            Id = from.Id,
            CreationTime = from.CreationTime,
            Name = from.Name,
            Description = from.Description,
            IsClosed = from.IsClosed,
            SubscribersCount = from.SubscribersCount,
            Administrators = from.Subscriptions
                .Select(subscription => subscription.User).Select(UserMapper.GetUserDto).ToList()
        };

    public static Community GetNewCommunity(CommunityCreateEditDto communityCreateEditDto)
        => new Community
        {
            Name = communityCreateEditDto.Name,
            Description = communityCreateEditDto.Description,
            CreationTime = DateTime.UtcNow,
            IsClosed = communityCreateEditDto.IsClosed
        };
}