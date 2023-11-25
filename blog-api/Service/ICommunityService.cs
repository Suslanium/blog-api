using blog_api.Data.Models;
using blog_api.Model;

namespace blog_api.Service;

public interface ICommunityService
{
    public Task<List<CommunityDto>> GetCommunityList();

    public Task<List<CommunityDto>> GetUserCommunities(Guid userGuid);

    public Task<CommunityFullDto> GetCommunityDetails(Guid communityGuid);

    public Task SubscribeUser(Guid userGuid, Guid communityGuid);

    public Task UnsubscribeUser(Guid userGuid, Guid communityGuid);
    
    public Task AddAdministrator(Guid callerGuid, Guid userGuid, Guid communityGuid);

    public Task RemoveAdministrator(Guid callerGuid, Guid userGuid, Guid communityGuid);

    public Task CreateCommunity(Guid creatorId, CommunityCreateEditDto communityCreateEditDto);

    public Task<CommunityRole> GetUserRole(Guid userGuid, Guid communityGuid);
}