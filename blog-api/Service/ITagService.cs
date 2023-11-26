using blog_api.Model;

namespace blog_api.Service;

public interface ITagService
{
    public Task CreateTag(TagCreationDto tagCreationDto);

    public Task<List<TagDto>> GetTagList();
}