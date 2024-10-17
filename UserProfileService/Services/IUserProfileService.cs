using UserProfileService.Dtos;

namespace UserProfileService.Services
{
    public interface IUserProfileService
    {
        Task RegisterUserAsync(UserProfileDto userProfile);
        Task<UserProfileDto?> GetUserProfileAsync(Guid id);
        Task FollowUserAsync(Guid userId, Guid userIdToFollow);
        Task UnfollowUserAsync(Guid userId, Guid userIdToUnfollow);
        Task<List<Guid>> GetFollowersAsync(Guid userId);
    }
}