using blog_api.Data;
using blog_api.Data.Models;
using blog_api.Exception;
using blog_api.Model;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Service;

public class PostService(BlogDbContext dbContext, FiasDbContext fiasDbContext) : IPostService
{
    private record PostInfo(PostFullDto Post, bool UserHasAccessToPost)
    {
        public readonly PostFullDto Post = Post;
        public readonly bool UserHasAccessToPost = UserHasAccessToPost;
    }

    public async Task<PostPagedListDto> GetPostList(Guid? userId, List<Guid>? tags, string? authorName,
        int? minReadingTime, int? maxReadingTime,
        SortingOption? sorting, bool onlyUserCommunities, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new BlogApiException(400, "Page number should be at least 1");

        if (pageSize < 1)
            throw new BlogApiException(400, "Page size should be at least 1");

        if (userId == null && onlyUserCommunities)
            throw new BlogApiException(401, "Authorization is required to access user communities");

        IQueryable<Post> postsQueryable = dbContext.Posts;

        if (userId == null)
            postsQueryable = postsQueryable.Where(post => post.Community == null || !post.Community.IsClosed);
        else
            postsQueryable = postsQueryable.Where(post =>
                post.Community == null || !post.Community.IsClosed ||
                post.Community.Subscriptions.Any(subscription => subscription.UserId == userId));

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
                SortingOption.LikeDesc => postsQueryable.OrderByDescending(post => post.Likes.Count),
                SortingOption.LikeAsc => postsQueryable.OrderBy(post => post.Likes.Count),
                _ => postsQueryable
            };
        if (onlyUserCommunities)
            postsQueryable = postsQueryable.Where(post =>
                post.Community != null &&
                post.Community.Subscriptions.Any(subscription => subscription.UserId == userId));

        var posts = await postsQueryable.Skip(pageSize * (pageNumber - 1)).Take(pageSize).Select(post => new PostDto
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
        }).ToListAsync();

        return new PostPagedListDto
        {
            Posts = posts,
            PaginationInfo = new PageInfoDto
            {
                CurrentPage = pageNumber,
                PageCount = (int)Math.Ceiling((float)await postsQueryable.CountAsync() / pageSize),
                Size = posts.Count
            }
        };
    }

    public async Task<PostFullDto> GetPostInfo(Guid? userId, Guid postId)
    {
        var postInfo = await dbContext.Posts.Where(post => post.Id == postId).Select(post => new PostInfo(
            new PostFullDto
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
            },
            post.Community == null || !post.Community.IsClosed || userId != null &&
            post.Community.Subscriptions.Any(subscription => subscription.UserId == userId)
        )).FirstOrDefaultAsync();

        if (postInfo == null)
            throw new BlogApiException(400, $"Post with Guid {postId} does not exist");

        if (!postInfo.UserHasAccessToPost)
            throw new BlogApiException(403, "User does not have access to the post");

        return postInfo.Post;
    }

    public async Task CreateUserPost(Guid userId, PostCreateEditDto createDto)
    {
        if (createDto.AddressId != null)
        {
            var addrObjectCount =
                await fiasDbContext.AsAddrObjs.CountAsync(obj =>
                    obj.Isactual == 1 && obj.Objectguid == createDto.AddressId);
            var houseCount = await fiasDbContext.AsHouses.CountAsync(house =>
                house.Isactual == 1 && house.Objectguid == createDto.AddressId);
            if (houseCount < 1 && addrObjectCount < 1)
                throw new BlogApiException(400, $"Address with Guid {createDto.AddressId} does not exist");
        }

        var tags = createDto.Tags.Select(tagGuid =>
        {
            var tag = dbContext.Tags.Find(tagGuid);
            if (tag == null)
                throw new BlogApiException(400, $"Tag with Guid {tagGuid} does not exist");
            return tag;
        });

        var post = new Post
        {
            CreationTime = DateTime.UtcNow,
            Title = createDto.Title,
            Description = createDto.Description,
            ReadingTime = createDto.ReadingTime,
            ImageUri = createDto.ImageUri,
            AddressId = createDto.AddressId,
            AuthorId = userId,
            CommunityId = null
        };
        post.Tags.AddRange(tags);
        dbContext.Posts.Add(post);

        await dbContext.SaveChangesAsync();
    }

    public async Task EditPost(Guid userId, Guid postId, PostCreateEditDto editDto)
    {
        var post = await dbContext.Posts.Include(postEntity => postEntity.Tags)
            .FirstOrDefaultAsync(postEntity => postEntity.Id == postId);
        if (post == null)
            throw new BlogApiException(400, $"Post with Guid {postId} does not exist");

        bool hasEditAccess;
        if (post.CommunityId != null)
            hasEditAccess = await dbContext.Communities.AnyAsync(community =>
                community.Id == post.CommunityId && community.Subscriptions.Any(subscription =>
                    subscription.UserId == userId && subscription.CommunityRole == CommunityRole.Administrator));
        else
            hasEditAccess = post.AuthorId == userId;

        if (!hasEditAccess)
            throw new BlogApiException(403, "User doesn't have rights to edit this post");

        var tags = editDto.Tags.Select(tagGuid =>
        {
            var tag = dbContext.Tags.Find(tagGuid);
            if (tag == null)
                throw new BlogApiException(400, $"Tag with Guid {tagGuid} does not exist");
            return tag;
        });

        post.Title = editDto.Title;
        post.Description = editDto.Description;
        post.ReadingTime = editDto.ReadingTime;
        post.EditedTime = DateTime.UtcNow;
        post.ImageUri = editDto.ImageUri;
        post.AddressId = editDto.AddressId;
        post.Tags.Clear();
        post.Tags.AddRange(tags);

        dbContext.Update(post);
        await dbContext.SaveChangesAsync();
    }

    public async Task LikePost(Guid userId, Guid postId)
    {
        var post = await dbContext.Posts.FindAsync(postId);
        if (post == null)
            throw new BlogApiException(400, "Post does not exist");

        if (post.CommunityId != null)
        {
            var canAccessPost = await dbContext.Communities.AnyAsync(community => community.Id == post.CommunityId &&
                (!community.IsClosed || community.Subscriptions.Any(subscription => subscription.UserId == userId)));
            if (!canAccessPost)
                throw new BlogApiException(403, "User doesn't have access to this post");
        }

        if (await dbContext.Posts.Where(postEntity => postEntity.Id == postId)
                .Select(postEntity => postEntity.Likes.Any(like => like.UserId == userId)).FirstAsync())
            throw new BlogApiException(400, "Post is already liked by user");

        post.Likes.Add(new LikedPosts { UserId = userId, PostId = postId });
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveLike(Guid userId, Guid postId)
    {
        var post = await dbContext.Posts.FindAsync(postId);
        if (post == null)
            throw new BlogApiException(400, "Post does not exist");

        if (post.CommunityId != null)
        {
            var canAccessPost = await dbContext.Communities.AnyAsync(community => community.Id == post.CommunityId &&
                (!community.IsClosed || community.Subscriptions.Any(subscription => subscription.UserId == userId)));
            if (!canAccessPost)
                throw new BlogApiException(403, "User doesn't have access to this post");
        }

        var like = await dbContext.Posts.Where(postEntity => postEntity.Id == postId)
            .Select(postEntity => postEntity.Likes.FirstOrDefault(like => like.UserId == userId)).FirstAsync();
        if (like == null)
            throw new BlogApiException(400, "Post is not liked by user");
        
        post.Likes.Remove(like);
        await dbContext.SaveChangesAsync();
    }
}