using blog_api.Data;
using blog_api.Data.Models;
using blog_api.Exception;
using blog_api.Model;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Service;

public class TagService(BlogDbContext dbContext) : ITagService
{
    public async Task CreateTag(TagCreationDto tagCreationDto)
    {
        if (await dbContext.Tags.FirstOrDefaultAsync(tag => tag.Name == tagCreationDto.Name) != null)
            throw new BlogApiArgumentException("Tag with same name already exists");

        dbContext.Tags.Add(new Tag
        {
            Name = tagCreationDto.Name,
            CreationTime = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<TagDto>> GetTagList()
    {
        return await dbContext.Tags.Select(tag => new TagDto
        {
            CreationTime = tag.CreationTime,
            Id = tag.Id,
            Name = tag.Name
        }).ToListAsync();
    }
}