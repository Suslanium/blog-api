using blog_api.Data.Models;

namespace blog_api.Model.Mapper;

public static class SubscriptionMapper
{
    public static Subscription GetSubscription(Guid userId, Guid communityId, CommunityRole role)
        => new Subscription
        {
            UserId = userId,
            CommunityId = communityId,
            CommunityRole = role
        };
}