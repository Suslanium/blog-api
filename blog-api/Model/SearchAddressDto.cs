namespace blog_api.Model;

public class SearchAddressDto
{
    public required long ObjectId { get; set; }

    public required Guid ObjectGuid { get; set; }
    
    public string? Text { get; set; }
}