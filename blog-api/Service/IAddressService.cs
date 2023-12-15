using blog_api.Model;

namespace blog_api.Service;

public interface IAddressService
{
    public Task<IEnumerable<SearchAddressDto>> Search(long parentObjectId, string? query);

    public Task<List<SearchAddressDto>> Chain(Guid objectGuid);
}