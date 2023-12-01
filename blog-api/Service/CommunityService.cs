using blog_api.Data;
using blog_api.Data.Models;
using blog_api.Exception;
using blog_api.Model;
using blog_api.Model.Mapper;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Service;

public class CommunityService(BlogDbContext dbContext, FiasDbContext fiasDbContext) : ICommunityService
{
    public Task<List<CommunityDto>> GetCommunityList()
    {
        return dbContext.Communities.Select(community => CommunityMapper.GetCommunityDto(community)).ToListAsync();
    }
    
    public async Task<List<CommunityUserDto>> GetUserCommunities(Guid userGuid)
    {
        var result = await dbContext.Users.Where(userEntity => userEntity.Id == userGuid).Select(userEntity =>
                userEntity.Subscriptions.Join(userEntity.SubscribedCommunities,
                    subscription => subscription.CommunityId,
                    community => community.Id,
                    (subscription, community) => CommunityMapper.GetCommunityUserDto(community, subscription)))
            .FirstAsync();
        return result.ToList();
    }

    public async Task<CommunityFullDto> GetCommunityDetails(Guid communityGuid)
    {
        var result = await dbContext.Communities.Where(community => community.Id == communityGuid).Include(community =>
                community.Subscriptions.Where(subscription =>
                    subscription.CommunityRole == CommunityRole.Administrator))
            .ThenInclude(subscription => subscription.User).FirstOrDefaultAsync();

        if (result == null)
            throw new BlogApiArgumentException("Community with specified id does not exist");

        return CommunityMapper.GetFullCommunityDto(result);
    }

    public async Task<PostPagedListDto> GetCommunityPosts(Guid? userId, Guid communityId, List<Guid>? tags,
        string? authorName, int? minReadingTime,
        int? maxReadingTime, SortingOption? sorting, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new BlogApiArgumentException("Page number should be at least 1");

        if (pageSize < 1)
            throw new BlogApiArgumentException("Page size should be at least 1");

        var canAccessPosts = await dbContext.Communities.Where(community => community.Id == communityId).Select(
                community => !community.IsClosed || userId != null &&
                    community.Subscriptions.Any(subscription => subscription.UserId == userId)).Cast<bool?>()
            .FirstOrDefaultAsync();

        switch (canAccessPosts)
        {
            case null:
                throw new BlogApiArgumentException($"Community with Guid {communityId} does not exist");
            case false:
                throw new BlogApiSecurityException("Cannot access posts of a closed community");
        }

        var postsQueryable = dbContext.Communities
            .Where(community => community.Id == communityId)
            .SelectMany(community => community.Posts);

        if (minReadingTime != null)
            postsQueryable = postsQueryable.Where(post => post.ReadingTime >= minReadingTime);
        if (maxReadingTime != null)
            postsQueryable = postsQueryable.Where(post => post.ReadingTime <= maxReadingTime);
        if (tags != null)
            postsQueryable = postsQueryable.Where(post =>
                post.Tags.Select(tag => tag.Id).Intersect(tags).Count() == tags.Count);
        if (authorName != null)
            postsQueryable = postsQueryable.Where(post => post.Author.FullName.Contains(authorName));
        if (sorting != null)
            postsQueryable = sorting switch
            {
                SortingOption.CreateDesc => postsQueryable.OrderByDescending(post => post.CreationTime),
                SortingOption.CreateAsc => postsQueryable.OrderBy(post => post.CreationTime),
                SortingOption.LikeDesc => postsQueryable.OrderByDescending(post => post.LikeCount),
                SortingOption.LikeAsc => postsQueryable.OrderBy(post => post.LikeCount),
                _ => postsQueryable
            };

        var postPageQueryable = postsQueryable.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
        var likes = await postPageQueryable.Select(post => userId != null && post.Likes.Any(like => like.UserId == userId))
            .ToListAsync();
        var postList = await postPageQueryable.Include(post => post.Author).Include(post => post.Community)
            .Include(post => post.Tags).ToListAsync();
        var posts = postList.Zip(likes, PostMapper.GetPostDto).ToList();
        var pageCount = (int)Math.Ceiling((float)await postsQueryable.CountAsync() / pageSize);

        return PostMapper.GetPostPagedListDto(pageNumber, posts, pageCount);
    }

    public async Task CreatePost(Guid userId, Guid communityId, PostCreateEditDto createDto)
    {
        if (createDto.AddressId != null)
        {
            var addrObjectCount =
                await fiasDbContext.AsAddrObjs.CountAsync(obj =>
                    obj.Isactual == 1 && obj.Objectguid == createDto.AddressId);
            var houseCount = await fiasDbContext.AsHouses.CountAsync(house =>
                house.Isactual == 1 && house.Objectguid == createDto.AddressId);
            if (houseCount < 1 && addrObjectCount < 1)
                throw new BlogApiArgumentException($"Address with Guid {createDto.AddressId} does not exist");
        }

        var tags = createDto.Tags.Select(tagGuid =>
        {
            var tag = dbContext.Tags.Find(tagGuid);
            if (tag == null)
                throw new BlogApiArgumentException($"Tag with Guid {tagGuid} does not exist");
            return tag;
        });

        if (await dbContext.Communities.FindAsync(communityId) == null)
            throw new BlogApiArgumentException($"Community with Guid {communityId} does not exist");

        if (!await dbContext.Communities.AnyAsync(community =>
                community.Id == communityId && community.Subscriptions
                    .Any(subscription => subscription.UserId == userId &&
                                         subscription.CommunityRole == CommunityRole.Administrator)))
            throw new BlogApiSecurityException("Only administrators of the community can create posts");

        var post = PostMapper.GetPostEntity(userId, communityId, createDto);
        post.Tags.AddRange(tags);
        dbContext.Posts.Add(post);

        await dbContext.SaveChangesAsync();
    }

    public async Task SubscribeUser(Guid userGuid, Guid communityGuid)
    {
        var community = await dbContext.Communities.FindAsync(communityGuid);
        if (community == null)
            throw new BlogApiArgumentException("Community with specified id does not exist");

        if (await dbContext.Subscriptions.FindAsync(communityGuid, userGuid) != null)
            throw new BlogApiArgumentException("User is already subscribed to specified community");

        dbContext.Subscriptions.Add(SubscriptionMapper.GetSubscription(userGuid, communityGuid, CommunityRole.Subscriber));
        community.SubscribersCount++;

        await dbContext.SaveChangesAsync();
    }

    public async Task UnsubscribeUser(Guid userGuid, Guid communityGuid)
    {
        var community = await dbContext.Communities.FindAsync(communityGuid);
        if (community == null)
            throw new BlogApiArgumentException("Community with specified id does not exist");

        var subscription = await dbContext.Subscriptions.FindAsync(communityGuid, userGuid);
        if (subscription == null)
            throw new BlogApiArgumentException("User is not subscribed to specified community");

        if (subscription.CommunityRole == CommunityRole.Administrator)
            throw new BlogApiArgumentException("Administrator cannot unsubscribe from the community");

        dbContext.Subscriptions.Remove(subscription);
        community.SubscribersCount--;

        await dbContext.SaveChangesAsync();
    }

    public async Task<CommunityRole> GetUserRole(Guid userGuid, Guid communityGuid)
    {
        if (await dbContext.Communities.FindAsync(communityGuid) == null)
            throw new BlogApiArgumentException("Community with specified id does not exist");

        var userSubscription = await dbContext.Subscriptions.FindAsync(communityGuid, userGuid);
        if (userSubscription == null)
            throw new BlogApiArgumentException("User is not subscribed to specified community");

        return userSubscription.CommunityRole;
    }

    public async Task AddAdministrator(Guid callerGuid, Guid userGuid, Guid communityGuid)
    {
        var community = await dbContext.Communities.FindAsync(communityGuid);
        if (community == null)
            throw new BlogApiArgumentException("Community with specified id does not exist");

        if (await dbContext.Users.FindAsync(userGuid) == null)
            throw new BlogApiArgumentException("User with specified id does not exist");

        var callerSubscription = await dbContext.Subscriptions.FindAsync(communityGuid, callerGuid);
        if (callerSubscription is not { CommunityRole: CommunityRole.Administrator })
            throw new BlogApiSecurityException(
                "The calling user does not have administrative rights in this community");

        if (callerGuid == userGuid)
            throw new BlogApiArgumentException("User is already an administrator");

        var userSubscription = await dbContext.Subscriptions.FindAsync(communityGuid, userGuid);
        if (userSubscription == null)
        {
            dbContext.Subscriptions.Add(SubscriptionMapper.GetSubscription(userGuid, communityGuid, CommunityRole.Administrator));
            community.SubscribersCount++;
        }
        else if (userSubscription.CommunityRole == CommunityRole.Administrator)
            throw new BlogApiArgumentException("User is already an administrator");
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
            throw new BlogApiArgumentException("Community with specified id does not exist");

        if (await dbContext.Users.FindAsync(userGuid) == null)
            throw new BlogApiArgumentException("User with specified id does not exist");

        var callerSubscription = await dbContext.Subscriptions.FindAsync(communityGuid, callerGuid);
        if (callerSubscription is not { CommunityRole: CommunityRole.Administrator })
            throw new BlogApiSecurityException(
                "The calling user does not have administrative rights in this community");

        if (await dbContext.Subscriptions.CountAsync(subscription =>
                subscription.CommunityId == communityGuid &&
                subscription.CommunityRole == CommunityRole.Administrator) <= 1)
            throw new BlogApiArgumentException("A community must have at least one administrator");

        if (callerGuid != userGuid)
        {
            var userSubscription = await dbContext.Subscriptions.FindAsync(communityGuid, userGuid);
            if (userSubscription is not { CommunityRole: CommunityRole.Administrator })
                throw new BlogApiArgumentException("Specified user is not an admin in this community");
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
        var community = CommunityMapper.GetNewCommunity(communityCreateEditDto);
        dbContext.Communities.Add(community);
        dbContext.Subscriptions.Add(SubscriptionMapper.GetSubscription(creatorId, community.Id, CommunityRole.Administrator));
        await dbContext.SaveChangesAsync();
    }

    public async Task EditCommunity(Guid editorId, Guid communityId, CommunityCreateEditDto editDto)
    {
        var community = await dbContext.Communities.FindAsync(communityId);

        if (community == null)
            throw new BlogApiArgumentException($"Community with Guid {communityId} does not exist");

        if (!await dbContext.Communities.AnyAsync(communityEntity =>
                communityEntity.Id == communityId && communityEntity.Subscriptions
                    .Any(subscription => subscription.UserId == editorId &&
                                         subscription.CommunityRole == CommunityRole.Administrator)))
            throw new BlogApiSecurityException("User doesn't have rights to edit this community");

        community.Name = editDto.Name;
        community.Description = editDto.Description;
        community.IsClosed = editDto.IsClosed;

        dbContext.Update(community);
        await dbContext.SaveChangesAsync();
    }
}