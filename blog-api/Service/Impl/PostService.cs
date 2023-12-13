using blog_api.Data;
using blog_api.Data.Models;
using blog_api.Exception;
using blog_api.Model;
using blog_api.Model.Mapper;
using blog_api.Service.Helper;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Service.Impl;

public class PostService(BlogDbContext dbContext, FiasDbContext fiasDbContext) : IPostService
{
    private record PostInfo(Post Post, bool UserHasAccessToPost, bool HasLike)
    {
        public readonly Post Post = Post;
        public readonly bool UserHasAccessToPost = UserHasAccessToPost;
        public readonly bool HasLike = HasLike;
    }

    public async Task<PostPagedListDto> GetPostList(Guid? userId, List<Guid>? tags, string? authorName,
        int? minReadingTime, int? maxReadingTime,
        SortingOption? sorting, bool onlyUserCommunities, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new BlogApiArgumentException("Page number should be at least 1");

        if (pageSize < 1)
            throw new BlogApiArgumentException("Page size should be at least 1");

        if (userId == null && onlyUserCommunities)
            throw new BlogApiUnauthorizedAccessException("Authorization is required to access user communities");

        IQueryable<Post> postsQueryable = dbContext.Posts;

        if (userId == null)
            postsQueryable = postsQueryable.Where(post => post.Community == null || !post.Community.IsClosed);
        else
            postsQueryable = postsQueryable.Where(post =>
                post.Community == null || !post.Community.IsClosed ||
                post.Community.Subscriptions.Any(subscription => subscription.UserId == userId));

        postsQueryable = postsQueryable.FilterPosts(tags, authorName, minReadingTime, maxReadingTime, sorting);
        
        if (onlyUserCommunities)
            postsQueryable = postsQueryable.Where(post =>
                post.Community != null &&
                post.Community.Subscriptions.Any(subscription => subscription.UserId == userId));

        var postPageQueryable = postsQueryable.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
        var likes = await postPageQueryable
            .Select(post => userId != null && post.Likes.Any(like => like.UserId == userId))
            .ToListAsync();
        var postList = await postPageQueryable.Include(post => post.Author)
            .Include(post => post.Community)
            .Include(post => post.Tags).ToListAsync();
        var posts = postList.Zip(likes, PostMapper.GetPostDto).ToList();
        var pageCount = (int)Math.Ceiling((float)await postsQueryable.CountAsync() / pageSize);

        return PostMapper.GetPostPagedListDto(pageNumber, posts, pageCount);
    }

    public async Task<PostFullDto> GetPostInfo(Guid? userId, Guid postId)
    {
        var postInfo = await dbContext.Posts.Where(post => post.Id == postId)
            .Include(post => post.Author)
            .Include(post => post.Community)
            .Include(post => post.Tags)
            .Include(post => post.Comments.Where(comment => comment.ParentCommentId == null))
            .ThenInclude(comment => comment.Author).Select(post => new PostInfo(
                post, 
                post.Community == null || !post.Community.IsClosed || userId != null &&
                post.Community.Subscriptions.Any(subscription => subscription.UserId == userId),
                userId != null && post.Likes.Any(like => like.UserId == userId)
            )).FirstOrDefaultAsync();

        if (postInfo == null)
            throw new BlogApiArgumentException($"Post with Guid {postId} does not exist");

        if (!postInfo.UserHasAccessToPost)
            throw new BlogApiSecurityException("User does not have access to the post");

        return PostMapper.GetPostFullDto(postInfo.Post, postInfo.HasLike);
    }

    public async Task CreateUserPost(Guid userId, PostCreateEditDto createDto)
    {
        if (createDto.AddressId != null)
            await fiasDbContext.EnsureAddressExists((Guid)createDto.AddressId);

        var tags = dbContext.GetTagsFromDto(createDto);

        var post = PostMapper.GetPostEntity(userId, null, createDto);
        post.Tags.AddRange(tags);
        dbContext.Posts.Add(post);

        await dbContext.SaveChangesAsync();
    }

    public async Task EditPost(Guid userId, Guid postId, PostCreateEditDto editDto)
    {
        var post = await dbContext.Posts.Include(postEntity => postEntity.Tags)
            .FirstOrDefaultAsync(postEntity => postEntity.Id == postId);
        if (post == null)
            throw new BlogApiArgumentException($"Post with Guid {postId} does not exist");

        bool hasEditAccess;
        if (post.CommunityId != null)
            hasEditAccess = await dbContext.UserHasAdministrativeRights((Guid)post.CommunityId, userId);
        else
            hasEditAccess = post.AuthorId == userId;

        if (!hasEditAccess)
            throw new BlogApiSecurityException("User doesn't have rights to edit this post");
        
        if (editDto.AddressId != null)
            await fiasDbContext.EnsureAddressExists((Guid)editDto.AddressId);

        var tags = dbContext.GetTagsFromDto(editDto);

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
            throw new BlogApiArgumentException("Post does not exist");

        if (post.CommunityId != null)
        {
            var canAccessPost = await dbContext.UserCanAccessCommunityPost((Guid)post.CommunityId, userId);
            if (!canAccessPost)
                throw new BlogApiSecurityException("User doesn't have access to this post");
        }

        if (await dbContext.Posts.Where(postEntity => postEntity.Id == postId)
                .Select(postEntity => postEntity.Likes.Any(like => like.UserId == userId)).FirstAsync())
            throw new BlogApiArgumentException("Post is already liked by user");

        post.Likes.Add(new LikedPosts { UserId = userId, PostId = postId });
        post.LikeCount++;
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveLike(Guid userId, Guid postId)
    {
        var post = await dbContext.Posts.FindAsync(postId);
        if (post == null)
            throw new BlogApiArgumentException("Post does not exist");

        if (post.CommunityId != null)
        {
            var canAccessPost = await dbContext.UserCanAccessCommunityPost((Guid)post.CommunityId, userId);
            if (!canAccessPost)
                throw new BlogApiSecurityException("User doesn't have access to this post");
        }

        var like = await dbContext.Posts.Where(postEntity => postEntity.Id == postId)
            .Select(postEntity => postEntity.Likes.FirstOrDefault(like => like.UserId == userId)).FirstAsync();
        if (like == null)
            throw new BlogApiArgumentException("Post is not liked by user");

        post.Likes.Remove(like);
        post.LikeCount--;
        await dbContext.SaveChangesAsync();
    }
}