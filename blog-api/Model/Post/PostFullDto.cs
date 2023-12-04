namespace blog_api.Model;

public class PostFullDto
{
    public required Guid Id { get; set; }
    
    public required DateTime CreationTime { get; set; }
    
    public DateTime? EditedTime { get; set; }
    
    public required string Title { get; set; }
    
    public required string Description { get; set; }
    
    public required int ReadingTime { get; set; }
    
    public string? ImageUri { get; set; }
    
    public required Guid AuthorId { get; set; }
    
    public required string AuthorName { get; set; }
    
    public Guid? CommunityId { get; set; }
    
    public string? CommunityName { get; set; }
    
    public Guid? AddressId { get; set; }
    
    public required int LikesCount { get; set; }
    
    public required bool HasLike { get; set; }
    
    public required int CommentsCount { get; set; }
    
    public required List<TagDto> Tags { get; set; }
    
    public required List<CommentDto> Comments { get; set; }
}