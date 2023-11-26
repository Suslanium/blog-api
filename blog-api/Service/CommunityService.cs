using blog_api.Data;
using blog_api.Data.Models;
using blog_api.Exception;
using blog_api.Model;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Service;

public class CommunityService(BlogDbContext dbContext) : ICommunityService
{
    //TODO add pagination
    public async Task<List<CommunityDto>> GetCommunityList()
    {
        return await dbContext.Communities.Select(community => new CommunityDto
        {
            Id = community.Id,
            CreationTime = community.CreationTime,
            Name = community.Name,
            Description = community.Description,
            IsClosed = community.IsClosed,
            SubscribersCount = community.Subscribers.Count
        }).ToListAsync();
    }

    //TODO add pagination
    public async Task<List<CommunityUserDto>> GetUserCommunities(Guid userGuid)
    {
        var user = await dbContext.Users
            .Include(user => user.Subscriptions)
            .Include(user => user.SubscribedCommunities)
            .ThenInclude(community => community.Subscriptions)
            .FirstAsync(user => user.Id == userGuid);

        return user.Subscriptions
            .Join(user.SubscribedCommunities, subscription => subscription.CommunityId,
                community => community.Id, (subscription, community) => new CommunityUserDto
                {
                    Id = community.Id,
                    CreationTime = community.CreationTime,
                    Name = community.Name,
                    Description = community.Description,
                    IsClosed = community.IsClosed,
                    SubscribersCount = community.Subscriptions.Count,
                    UserRole = subscription.CommunityRole
                }).ToList();
    }

    public async Task<CommunityFullDto> GetCommunityDetails(Guid communityGuid)
    {
        var result = await dbContext.Communities.Where(community => community.Id == communityGuid).Select(community =>
            new CommunityFullDto
            {
                Id = community.Id,
                CreationTime = community.CreationTime,
                Name = community.Name,
                Description = community.Description,
                IsClosed = community.IsClosed,
                SubscribersCount = community.Subscribers.Count,
                Administrators = community.Subscriptions
                    .Where(subscription => subscription.CommunityRole == CommunityRole.Administrator).Join(
                        community.Subscribers, subscription => subscription.UserId, user => user.Id,
                        (subscription, user) => new UserDto
                        {
                            Id = user.Id,
                            FullName = user.FullName,
                            Gender = user.Gender,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            BirthDate = user.BirthDate,
                            CreationTime = user.CreationTime
                        }).ToList()
            }).FirstOrDefaultAsync();
        
        if (result == null)
            throw new BlogApiException(400, "Community with specified id does not exist");
        
        return result;
    }

    public async Task SubscribeUser(Guid userGuid, Guid communityGuid)
    {
        if (await dbContext.Communities.FindAsync(communityGuid) == null)
            throw new BlogApiException(400, "Community with specified id does not exist");

        if (await dbContext.Subscriptions.FindAsync(communityGuid, userGuid) != null)
            throw new BlogApiException(400, "User is already subscribed to specified community");

        dbContext.Subscriptions.Add(new Subscription
        {
            UserId = userGuid,
            CommunityId = communityGuid,
            CommunityRole = CommunityRole.Subscriber
        });

        await dbContext.SaveChangesAsync();
    }

    public async Task UnsubscribeUser(Guid userGuid, Guid communityGuid)
    {
        if (await dbContext.Communities.FindAsync(communityGuid) == null)
            throw new BlogApiException(400, "Community with specified id does not exist");

        var subscription = await dbContext.Subscriptions.FindAsync(communityGuid, userGuid);
        if (subscription == null)
            throw new BlogApiException(400, "User is not subscribed to specified community");

        if (subscription.CommunityRole == CommunityRole.Administrator)
            throw new BlogApiException(400, "Administrator cannot unsubscribe from the community");

        dbContext.Subscriptions.Remove(subscription);

        await dbContext.SaveChangesAsync();
    }

    public async Task<CommunityRole> GetUserRole(Guid userGuid, Guid communityGuid)
    {
        if (await dbContext.Communities.FindAsync(communityGuid) == null)
            throw new BlogApiException(400, "Community with specified id does not exist");

        var userSubscription = await dbContext.Subscriptions.FindAsync(communityGuid, userGuid);
        if (userSubscription == null)
            throw new BlogApiException(400, "User is not subscribed to specified community");

        return userSubscription.CommunityRole;
    }

    public async Task AddAdministrator(Guid callerGuid, Guid userGuid, Guid communityGuid)
    {
        if (await dbContext.Communities.FindAsync(communityGuid) == null)
            throw new BlogApiException(400, "Community with specified id does not exist");

        if (await dbContext.Users.FindAsync(userGuid) == null)
            throw new BlogApiException(400, "User with specified id does not exist");

        var callerSubscription = await dbContext.Subscriptions.FindAsync(communityGuid, callerGuid);
        if (callerSubscription is not { CommunityRole: CommunityRole.Administrator })
            throw new BlogApiException(403, "The calling user does not have administrative rights in this community");

        if (callerGuid == userGuid)
            throw new BlogApiException(400, "User is already an administrator");

        var userSubscription = await dbContext.Subscriptions.FindAsync(communityGuid, userGuid);
        if (userSubscription == null)
            dbContext.Subscriptions.Add(new Subscription
            {
                UserId = userGuid,
                CommunityId = communityGuid,
                CommunityRole = CommunityRole.Administrator
            });
        else if (userSubscription.CommunityRole == CommunityRole.Administrator)
            throw new BlogApiException(400, "User is already an administrator");
        else
        {
            userSubscription.CommunityRole = CommunityRole.Administrator;
            dbContext.Subscriptions.Update(userSubscription);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveAdministrator(Guid callerGuid, Guid userGuid, Guid communityGuid)
    {
        if (await dbContext.Communities.FindAsync(communityGuid) == null)
            throw new BlogApiException(400, "Community with specified id does not exist");

        if (await dbContext.Users.FindAsync(userGuid) == null)
            throw new BlogApiException(400, "User with specified id does not exist");

        var callerSubscription = await dbContext.Subscriptions.FindAsync(communityGuid, callerGuid);
        if (callerSubscription is not { CommunityRole: CommunityRole.Administrator })
            throw new BlogApiException(403, "The calling user does not have administrative rights in this community");

        if (await dbContext.Subscriptions.CountAsync(subscription =>
                subscription.CommunityId == communityGuid &&
                subscription.CommunityRole == CommunityRole.Administrator) <= 1)
            throw new BlogApiException(400, "A community must have at least one administrator");

        if (callerGuid != userGuid)
        {
            var userSubscription = await dbContext.Subscriptions.FindAsync(communityGuid, userGuid);
            if (userSubscription is not { CommunityRole: CommunityRole.Administrator })
                throw new BlogApiException(400, "Specified user is not an admin in this community");
            userSubscription.CommunityRole = CommunityRole.Subscriber;
        }
        else
        {
            callerSubscription.CommunityRole = CommunityRole.Subscriber;
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task CreateCommunity(Guid creatorId, CommunityCreateEditDto communityCreateEditDto)
    {
        var community = new Community
        {
            Name = communityCreateEditDto.Name,
            Description = communityCreateEditDto.Description,
            CreationTime = DateTime.UtcNow,
            IsClosed = communityCreateEditDto.IsClosed
        };
        dbContext.Communities.Add(community);
        dbContext.Subscriptions.Add(new Subscription
        {
            UserId = creatorId,
            CommunityId = community.Id,
            CommunityRole = CommunityRole.Administrator
        });
        await dbContext.SaveChangesAsync();
    }
}