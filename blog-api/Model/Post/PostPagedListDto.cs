namespace blog_api.Model;

public class PostPagedListDto
{
    public required List<PostDto> Posts { get; set; }
    
    public required PageInfoDto PaginationInfo { get; set; }
}