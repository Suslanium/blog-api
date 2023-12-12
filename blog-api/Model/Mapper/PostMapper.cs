using blog_api.Data.Models;

namespace blog_api.Model.Mapper;

public static class PostMapper
{
    public static PostDto GetPostDto(Post from, bool hasLike)
        => new PostDto
        {
            Id = from.Id,
            CreationTime = from.CreationTime,
            EditedTime = from.EditedTime,
            Title = from.Title,
            Description = from.Description,
            ReadingTime = from.ReadingTime,
            ImageUri = from.ImageUri,
            AuthorId = from.AuthorId,
            AuthorName = from.Author.FullName,
            CommunityId = from.CommunityId,
            CommunityName = from.Community?.Name,
            AddressId = from.AddressId,
            LikesCount = from.LikeCount,
            HasLike = hasLike,
            CommentsCount = from.CommentCount,
            Tags = from.Tags.Select(TagMapper.GetTagDto).ToList()
        };

    public static PostFullDto GetPostFullDto(Post from, bool hasLike)
        => new PostFullDto
        {
            Id = from.Id,
            CreationTime = from.CreationTime,
            EditedTime = from.EditedTime,
            Title = from.Title,
            Description = from.Description,
            ReadingTime = from.ReadingTime,
            ImageUri = from.ImageUri,
            AuthorId = from.AuthorId,
            AuthorName = from.Author.FullName,
            CommunityId = from.CommunityId,
            CommunityName = from.Community?.Name,
            AddressId = from.AddressId,
            LikesCount = from.LikeCount,
            HasLike = hasLike,
            CommentsCount = from.CommentCount,
            Tags = from.Tags.Select(TagMapper.GetTagDto).ToList(),
            Comments = from.Comments.OrderBy(comment => comment.CreationTime).Select(CommentMapper.GetCommentDto).ToList()
        };

    public static PostPagedListDto GetPostPagedListDto(int pageNumber, List<PostDto> posts, int pageCount)
        => new PostPagedListDto
        {
            Posts = posts,
            PaginationInfo = new PageInfoDto
            {
                CurrentPage = pageNumber,
                PageCount = pageCount,
                Size = posts.Count
            }
        };

    public static Post GetPostEntity(Guid userId, Guid? communityId, PostCreateEditDto createDto)
        => new Post
        {
            CreationTime = DateTime.UtcNow,
            Title = createDto.Title,
            Description = createDto.Description,
            ReadingTime = createDto.ReadingTime,
            ImageUri = createDto.ImageUri,
            AddressId = createDto.AddressId,
            AuthorId = userId,
            CommunityId = communityId
        };
}