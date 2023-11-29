namespace blog_api.Model;

public class PageInfoDto
{
    public required int Size { get; set; }
    
    public required int PageCount { get; set; }
    
    public required int CurrentPage { get; set; }
}