using blog_api.Data;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Service.Helper;

public static class CommentHelper
{
    public static Task<bool> UserHasAccessToComments(this BlogDbContext dbContext, Guid postId, Guid userId)
    {
        return dbContext.Posts.Where(postEntity => postEntity.Id == postId)
            .Select(postEntity =>
                postEntity.Community == null || !postEntity.Community.IsClosed ||
                postEntity.Community.Subscriptions.Any(subscription => subscription.UserId == userId))
            .FirstOrDefaultAsync();
    }
}