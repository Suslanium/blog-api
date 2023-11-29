using blog_api.Data;
using blog_api.Data.Models;
using blog_api.Exception;
using blog_api.Model;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Service;

public class CommunityService(BlogDbContext dbContext, FiasDbContext fiasDbContext) : ICommunityService
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
        var result = await dbContext.Users.Where(userEntity => userEntity.Id == userGuid).Select(userEntity =>
            userEntity.Subscriptions.Join(userEntity.SubscribedCommunities, subscription => subscription.CommunityId,
                community => community.Id, (subscription, community) => new CommunityUserDto
                {
                    Id = community.Id,
                    CreationTime = community.CreationTime,
                    Name = community.Name,
                    Description = community.Description,
                    IsClosed = community.IsClosed,
                    SubscribersCount = community.Subscriptions.Count,
                    UserRole = subscription.CommunityRole
                })).FirstAsync();
        return result.ToList();
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
                    .Where(subscription => subscription.CommunityRole == CommunityRole.Administrator)
                    .Select(subscription => subscription.User).Select(user => new UserDto
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

    public async Task<PostPagedListDto> GetCommunityPosts(Guid? userId, Guid communityId, List<Guid>? tags,
        string? authorName, int? minReadingTime,
        int? maxReadingTime, SortingOption? sorting, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new BlogApiException(400, "Page number should be at least 1");

        if (pageSize < 1)
            throw new BlogApiException(400, "Page size should be at least 1");

        var canAccessPosts = await dbContext.Communities.Where(community => community.Id == communityId).Select(
                community => !community.IsClosed || userId != null &&
                    community.Subscriptions.Any(subscription => subscription.UserId == userId)).Cast<bool?>()
            .FirstOrDefaultAsync();

        switch (canAccessPosts)
        {
            case null:
                throw new BlogApiException(400, $"Community with Guid {communityId} does not exist");
            case false:
                throw new BlogApiException(403, "Cannot access posts of a closed community");
        }

        var postsQueryable = dbContext.Communities.Where(community => community.Id == communityId)
            .Select(community => community.Posts);

        if (minReadingTime != null)
            postsQueryable =
                postsQueryable.Select(posts => posts.Where(post => post.ReadingTime >= minReadingTime).ToList());
        if (maxReadingTime != null)
            postsQueryable =
                postsQueryable.Select(posts => posts.Where(post => post.ReadingTime <= maxReadingTime).ToList());
        if (tags != null)
            postsQueryable = postsQueryable.Select(posts => posts.Where(post =>
                post.Tags.Select(tag => tag.Id).Intersect(tags).Count() == tags.Count).ToList());
        if (authorName != null)
            postsQueryable =
                postsQueryable.Select(posts => posts.Where(post => post.Author.FullName.Contains(authorName)).ToList());
        if (sorting != null)
            postsQueryable = sorting switch
            {
                SortingOption.CreateDesc => postsQueryable.Select(posts =>
                    posts.OrderByDescending(post => post.CreationTime).ToList()),
                SortingOption.CreateAsc => postsQueryable.Select(posts =>
                    posts.OrderBy(post => post.CreationTime).ToList()),
                SortingOption.LikeDesc => postsQueryable.Select(posts =>
                    posts.OrderByDescending(post => post.Likes.Count).ToList()),
                SortingOption.LikeAsc => postsQueryable.Select(
                    posts => posts.OrderBy(post => post.Likes.Count).ToList()),
                _ => postsQueryable
            };

        var posts = await postsQueryable.Skip(pageSize * (pageNumber - 1)).Take(pageSize).Select(posts =>
                posts.Select(post => new PostDto
                {
                    Id = post.Id,
                    CreationTime = post.CreationTime,
                    EditedTime = post.EditedTime,
                    Title = post.Title,
                    Description = post.Description,
                    ReadingTime = post.ReadingTime,
                    ImageUri = post.ImageUri,
                    AuthorId = post.AuthorId,
                    AuthorName = post.Author.FullName,
                    CommunityId = post.CommunityId,
                    CommunityName = post.Community != null ? post.Community.Name : null,
                    AddressId = post.AddressId,
                    LikesCount = post.Likes.Count,
                    HasLike =
                        userId != null && post.Likes.Any(like => like.UserId == userId),
                    CommentsCount = 0,
                    Tags = post.Tags.Select(tag => new TagDto
                    {
                        Id = tag.Id,
                        CreationTime = tag.CreationTime,
                        Name = tag.Name
                    }).ToList()
                }).ToList())
            .FirstOrDefaultAsync();

        return new PostPagedListDto
        {
            Posts = posts ?? new List<PostDto>(),
            PaginationInfo = new PageInfoDto
            {
                CurrentPage = pageNumber,
                PageCount = (int)Math.Ceiling((float)await postsQueryable.CountAsync() / pageSize),
                Size = posts?.Count ?? 0
            }
        };
    }

    public async Task CreatePost(Guid userId, Guid communityId, PostCreateEditDto editDto)
    {
        if (editDto.AddressId != null)
        {
            var addrObjectCount =
                await fiasDbContext.AsAddrObjs.CountAsync(obj =>
                    obj.Isactual == 1 && obj.Objectguid == editDto.AddressId);
            var houseCount = await fiasDbContext.AsHouses.CountAsync(house =>
                house.Isactual == 1 && house.Objectguid == editDto.AddressId);
            if (houseCount < 1 && addrObjectCount < 1)
                throw new BlogApiException(400, $"Address with Guid {editDto.AddressId} does not exist");
        }

        var tags = editDto.Tags.Select(tagGuid =>
        {
            var tag = dbContext.Tags.Find(tagGuid);
            if (tag == null)
                throw new BlogApiException(400, $"Tag with Guid {tagGuid} does not exist");
            return tag;
        });

        if (await dbContext.Communities.FindAsync(communityId) == null)
            throw new BlogApiException(400, $"Community with Guid {communityId} does not exist");

        if (!await dbContext.Communities.AnyAsync(community =>
                community.Id == communityId && community.Subscriptions
                    .Any(subscription => subscription.UserId == userId &&
                                         subscription.CommunityRole == CommunityRole.Administrator)))
            throw new BlogApiException(403, "Only administrators of the community can create posts");

        var post = new Post
        {
            CreationTime = DateTime.UtcNow,
            Title = editDto.Title,
            Description = editDto.Description,
            ReadingTime = editDto.ReadingTime,
            ImageUri = editDto.ImageUri,
            AddressId = editDto.AddressId,
            AuthorId = userId,
            CommunityId = communityId
        };
        post.Tags.AddRange(tags);
        dbContext.Posts.Add(post);

        await dbContext.SaveChangesAsync();
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

    public async Task EditCommunity(Guid editorId, Guid communityId, CommunityCreateEditDto editDto)
    {
        var community = await dbContext.Communities.FindAsync(communityId);

        if (community == null)
            throw new BlogApiException(400, $"Community with Guid {communityId} does not exist");

        if (!await dbContext.Communities.AnyAsync(communityEntity =>
                communityEntity.Id == communityId && communityEntity.Subscriptions
                    .Any(subscription => subscription.UserId == editorId &&
                                         subscription.CommunityRole == CommunityRole.Administrator)))
            throw new BlogApiException(403, "User doesn't have rights to edit this community");

        community.Name = editDto.Name;
        community.Description = editDto.Description;
        community.IsClosed = editDto.IsClosed;

        dbContext.Update(community);
        await dbContext.SaveChangesAsync();
    }
}