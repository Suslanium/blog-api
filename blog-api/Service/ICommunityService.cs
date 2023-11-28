using blog_api.Data.Models;
using blog_api.Model;

namespace blog_api.Service;

public interface ICommunityService
{
    public Task<List<CommunityDto>> GetCommunityList();

    public Task<List<CommunityUserDto>> GetUserCommunities(Guid userGuid);

    public Task<CommunityFullDto> GetCommunityDetails(Guid communityGuid);

    public Task<PostPagedListDto> GetCommunityPosts(Guid? userId, Guid communityId, List<Guid>? tags, string? authorName,
        int? minReadingTime, int? maxReadingTime, SortingOption? sorting, int pageNumber, int pageSize);
    
    public Task CreatePost(Guid userId, Guid communityId, CreatePostDto postDto);

    public Task SubscribeUser(Guid userGuid, Guid communityGuid);

    public Task UnsubscribeUser(Guid userGuid, Guid communityGuid);

    public Task AddAdministrator(Guid callerGuid, Guid userGuid, Guid communityGuid);

    public Task RemoveAdministrator(Guid callerGuid, Guid userGuid, Guid communityGuid);

    public Task CreateCommunity(Guid creatorId, CommunityCreateEditDto communityCreateEditDto);

    public Task<CommunityRole> GetUserRole(Guid userGuid, Guid communityGuid);
}