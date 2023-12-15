using blog_api.Data.Models;

namespace blog_api.Model;

public class SearchAddressDto
{
    public required long ObjectId { get; set; }

    public required Guid ObjectGuid { get; set; }
    
    public string? Text { get; set; }
    
    public GarAddressLevel ObjectLevel { get; set; }
    
    public string? ObjectLevelText { get; set; }
}