using System.Linq.Expressions;
using blog_api.Data.Models;

namespace blog_api.Model.Mapper;

public static class CommunityMapper
{
    public static Expression<Func<Community, CommunityDto>> CommunityDtoConverter()
    {
        return community => new CommunityDto
        {
            Id = community.Id,
            CreationTime = community.CreationTime,
            Name = community.Name,
            Description = community.Description,
            IsClosed = community.IsClosed,
            SubscribersCount = community.Subscribers.Count
        };
    }
    
}