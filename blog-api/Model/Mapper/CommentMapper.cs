using blog_api.Data.Models;

namespace blog_api.Model.Mapper;

public static class CommentMapper
{
    public static CommentDto GetCommentDto(Comment from)
    {
        return new CommentDto
        {
            Id = from.Id,
            AuthorId = from.AuthorId,
            AuthorName = from.Author.FullName,
            Content = from.Content,
            CreationTime = from.CreationTime,
            DeleteTime = from.DeletedTime,
            ModifiedTime = from.ModifiedTime,
            ParentCommentId = from.ParentCommentId,
            SubCommentsCount = from.SubCommentCount
        };
    }
}