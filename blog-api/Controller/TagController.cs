using blog_api.Model;
using blog_api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace blog_api.Controller;

[ApiController]
[Route("api/tag")]
public class TagController(ITagService tagService) : ControllerBase
{
    [HttpGet]
    public async Task<List<TagDto>> GetTagList()
    {
        return await tagService.GetTagList();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateTag(TagCreationDto tagCreationDto)
    {
        await tagService.CreateTag(tagCreationDto);
        return Ok();
    }
}