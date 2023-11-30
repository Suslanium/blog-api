using blog_api.Data.Models;

namespace blog_api.Model.Mapper;

public class TagMapper
{
    public static TagDto GetTagDto(Tag from)
    {
        return new TagDto
        {
            Id = from.Id,
            CreationTime = from.CreationTime,
            Name = from.Name
        };
    }

    public static Tag GetNewTag(TagCreationDto from)
    {
        return new Tag
        {
            Name = from.Name,
            CreationTime = DateTime.UtcNow
        };
    }
}