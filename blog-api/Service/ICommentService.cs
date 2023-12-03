using blog_api.Model;

namespace blog_api.Service;

public interface ICommentService
{
    public Task<List<CommentDto>> GetCommentTree(Guid userId, Guid rootCommentId);
    
    public Task AddComment(Guid userId, Guid postId, CommentCreateDto commentCreateDto);

    public Task EditComment(Guid userId, Guid commentId, CommentUpdateDto commentUpdateDto);

    public Task DeleteComment(Guid userId, Guid commentId);
}