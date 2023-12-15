using blog_api.Data;
using blog_api.Data.Models;
using blog_api.Exception;
using blog_api.Model;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Service.Helper;

public static class PostHelper
{
    public static IQueryable<Post> FilterPosts(this IQueryable<Post> postsQueryable, List<Guid>? tags,
        string? authorName, int? minReadingTime, int? maxReadingTime, SortingOption? sorting)
    {
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
        return postsQueryable;
    }

    public static async Task EnsureAddressExists(this FiasDbContext fiasDbContext, Guid addressId)
    {
        var addrObjectCount =
            await fiasDbContext.AsAddrObjs.CountAsync(obj =>
                obj.Isactual == 1 && obj.Objectguid == addressId);
        var houseCount = await fiasDbContext.AsHouses.CountAsync(house =>
            house.Isactual == 1 && house.Objectguid == addressId);
        if (houseCount < 1 && addrObjectCount < 1)
            throw new BlogApiArgumentException($"Address with Guid {addressId} does not exist");
    }

    public static IEnumerable<Tag> GetTagsFromDto(this BlogDbContext dbContext, PostCreateEditDto createEditDto)
    {
        return createEditDto.Tags.Select(tagGuid =>
        {
            var tag = dbContext.Tags.Find(tagGuid);
            if (tag == null)
                throw new BlogApiArgumentException($"Tag with Guid {tagGuid} does not exist");
            return tag;
        });
    }

    public static Task<bool> UserCanAccessCommunityPost(this BlogDbContext dbContext, Guid communityId,
        Guid userId)
    {
        return dbContext.Communities.AnyAsync(community => community.Id == communityId && (!community.IsClosed ||
            community.Subscriptions.Any(subscription => subscription.UserId == userId)));
    }

    public static Task<bool> UserHasAdministrativeRights(this BlogDbContext dbContext, Guid communityId, Guid userId)
    {
        return dbContext.Communities.AnyAsync(community =>
            community.Id == communityId && community.Subscriptions
                .Any(subscription => subscription.UserId == userId &&
                                     subscription.CommunityRole == CommunityRole.Administrator));
    }
}