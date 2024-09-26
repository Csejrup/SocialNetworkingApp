using UserProfileService.Dtos;

namespace UserProfileService.Services
{
    public interface IUserProfileService
    {
        Task RegisterUserAsync(UserProfileDto userProfile);
        Task<UserProfileDto?> GetUserProfileAsync(int id);
        Task FollowUserAsync(int userId, int userIdToFollow);
        Task UnfollowUserAsync(int userId, int userIdToUnfollow);
        Task<List<int>> GetFollowersAsync(int userId);
        
        Task CreateDb();

    }
}