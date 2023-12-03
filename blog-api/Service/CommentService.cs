﻿using blog_api.Data;
using blog_api.Data.Models;
using blog_api.Exception;
using blog_api.Model;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Service;

public class CommentService(BlogDbContext dbContext) : ICommentService
{
    public Task<List<CommentDto>> GetCommentTree(Guid userId, Guid rootCommentId)
    {
        throw new NotImplementedException();
    }

    public async Task AddComment(Guid userId, Guid postId, CommentCreateDto commentCreateDto)
    {
        var post = await dbContext.Posts.FindAsync(postId);
        if (post == null)
            throw new BlogApiArgumentException($"Post with Guid {postId} does not exist");

        if (!await dbContext.Posts.Where(postEntity => postEntity.Id == postId)
                .Select(postEntity =>
                    postEntity.Community == null || !postEntity.Community.IsClosed ||
                    postEntity.Community.Subscriptions.Any(subscription => subscription.UserId == userId))
                .FirstOrDefaultAsync())
            throw new BlogApiSecurityException("User doesn't have access to specified post");

        Comment? parentComment = null;
        if (commentCreateDto.ParentCommentId != null)
        {
            parentComment = await dbContext.Comments.FirstOrDefaultAsync(comment =>
                comment.Id == commentCreateDto.ParentCommentId && comment.PostId == postId);
            if (parentComment == null)
                throw new BlogApiArgumentException(
                    $"Parent comment with Guid {commentCreateDto.ParentCommentId} either does not exist or does not belong to the specified post");
        }

        var comment = new Comment
        {
            AuthorId = userId,
            Content = commentCreateDto.Content,
            CreationTime = DateTime.UtcNow,
            ParentCommentId = commentCreateDto.ParentCommentId,
            PostId = postId
        };
        if (parentComment != null)
            parentComment.SubCommentCount++;
        post.CommentCount++;

        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync();
    }

    public async Task EditComment(Guid userId, Guid commentId, CommentUpdateDto commentUpdateDto)
    {
        var comment = await dbContext.Comments.FindAsync(commentId);
        if (comment == null || comment.DeletedTime != null)
            throw new BlogApiArgumentException($"Comment with Guid {commentId} does not exist");

        if (comment.AuthorId != userId)
            throw new BlogApiSecurityException("Only comment's author can edit their comment");

        if (!await dbContext.Posts.Where(postEntity => postEntity.Id == comment.PostId)
                .Select(postEntity =>
                    postEntity.Community == null || !postEntity.Community.IsClosed ||
                    postEntity.Community.Subscriptions.Any(subscription => subscription.UserId == userId))
                .FirstOrDefaultAsync())
            throw new BlogApiSecurityException("User doesn't have access to specified post");

        comment.Content = commentUpdateDto.Content;
        comment.ModifiedTime = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteComment(Guid userId, Guid commentId)
    {
        var comment = await dbContext.Comments.Include(commentEntity => commentEntity.ParentComment)
            .Include(commentEntity => commentEntity.Post)
            .FirstOrDefaultAsync(commentEntity => commentEntity.Id == commentId);
        
        if (comment == null || comment.DeletedTime != null)
            throw new BlogApiArgumentException($"Comment with Guid {commentId} does not exist");

        if (comment.AuthorId != userId)
            throw new BlogApiSecurityException("Only comment's author can delete their comment");

        if (!await dbContext.Posts.Where(postEntity => postEntity.Id == comment.PostId)
                .Select(postEntity =>
                    postEntity.Community == null || !postEntity.Community.IsClosed ||
                    postEntity.Community.Subscriptions.Any(subscription => subscription.UserId == userId))
                .FirstOrDefaultAsync())
            throw new BlogApiSecurityException("User doesn't have access to specified post");

        if (comment.SubCommentCount < 1)
        {
            if (comment.ParentComment != null)
                comment.ParentComment.SubCommentCount--;
            comment.Post.CommentCount--;
            dbContext.Comments.Remove(comment);
        }
        else
        {
            comment.DeletedTime = DateTime.UtcNow;
            comment.Content = string.Empty;
        }

        await dbContext.SaveChangesAsync();
    }
}