using blog_api.Data;
using blog_api.Exception;
using blog_api.Model;
using blog_api.Model.Mapper;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Service.Impl;

public class TagService(BlogDbContext dbContext) : ITagService
{
    public async Task CreateTag(TagCreationDto tagCreationDto)
    {
        if (await dbContext.Tags.FirstOrDefaultAsync(tag => tag.Name == tagCreationDto.Name) != null)
            throw new BlogApiArgumentException("Tag with same name already exists");

        dbContext.Tags.Add(TagMapper.GetNewTag(tagCreationDto));
        await dbContext.SaveChangesAsync();
    }

    public Task<List<TagDto>> GetTagList()
    {
        return dbContext.Tags.Select(tag => TagMapper.GetTagDto(tag)).ToListAsync();
    }
}