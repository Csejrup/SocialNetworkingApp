using UserProfileService.Models;

namespace UserProfileService.Repositories
{
    public interface IUserProfileRepository
    {
        Task AddUserProfileAsync(UserProfile userProfile);
        Task<UserProfile?> GetUserProfileByIdAsync(Guid id);
        Task<UserProfile?> GetUserProfileByUsernameAsync(string username); 
        Task UpdateUserProfileAsync(UserProfile userProfile);
        Task<List<UserProfile>> GetAllUserProfilesAsync();
        Task FollowUserAsync(Guid userId, Guid userIdToFollow);
        Task UnfollowUserAsync(Guid userId, Guid userIdToUnfollow);
        Task<List<Guid>> GetFollowersAsync(Guid userId);

    }
}