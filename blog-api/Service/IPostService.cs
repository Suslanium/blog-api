using blog_api.Model;

namespace blog_api.Service;

public interface IPostService
{
    public Task<PostPagedListDto> GetPostList(Guid? userId, List<Guid>? tags, string? authorName, int? minReadingTime,
        int? maxReadingTime, SortingOption? sorting, bool onlyUserCommunities, int pageNumber, int pageSize);
    
    public Task CreateUserPost(Guid userId, CreatePostDto postDto);

    public Task LikePost(Guid userId, Guid postId);

    public Task RemoveLike(Guid userId, Guid postId);
}