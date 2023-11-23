using blog_api.Model;
using blog_api.Service;
using Microsoft.AspNetCore.Mvc;

namespace blog_api.Controller;

[ApiController]
[Route("api/address")]
public class AddressController(IAddressService addressService): ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<SearchAddressDto>>> Search([FromQuery] long parentObjectId, [FromQuery] string? query)
    {
        var result = await addressService.Search(parentObjectId, query);
        return Ok(result);
    }
}