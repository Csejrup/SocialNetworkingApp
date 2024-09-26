using UserProfileService.Models;

namespace UserProfileService.Repositories
{
    public interface IUserProfileRepository
    {
        Task AddUserProfileAsync(UserProfile userProfile);
        Task<UserProfile?> GetUserProfileByIdAsync(int id);
        Task<UserProfile?> GetUserProfileByUsernameAsync(string username); 
        Task UpdateUserProfileAsync(UserProfile userProfile);
        Task<List<UserProfile>> GetAllUserProfilesAsync();
        Task FollowUserAsync(int userId, int userIdToFollow);
        Task UnfollowUserAsync(int userId, int userIdToUnfollow);
        Task<List<int>> GetFollowersAsync(int userId);
        
        Task RecreateDatabase();


    }
}