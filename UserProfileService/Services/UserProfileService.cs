using Monitoring;
using UserProfileService.Dtos;
using UserProfileService.Models;
using UserProfileService.Repositories;

namespace UserProfileService.Services
{
    public class UserProfileService(IUserProfileRepository userProfileRepository) : IUserProfileService
    {
        public async Task RegisterUserAsync(UserProfileDto userProfileDto)
        {
            using var activity = LoggingService.activitySource.StartActivity();

            var userProfile = new UserProfile
            {
                Id = userProfileDto.Id,
                Username = userProfileDto.Username,
                Email = userProfileDto.Email,
                Bio = userProfileDto.Bio
            };

            await userProfileRepository.AddUserProfileAsync(userProfile);
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int id)
        {
            
            using var activity = LoggingService.activitySource.StartActivity();
            var userProfile = await userProfileRepository.GetUserProfileByIdAsync(id);
            if (userProfile == null)
            {
                LoggingService.Log.AddContext().Debug("Could not find user with id: " + id);
                return null;
            }

            return new UserProfileDto
            {
                Id = userProfile.Id,
                Username = userProfile.Username,
                Email = userProfile.Email,
                Bio = userProfile.Bio
            };
        }

        public async Task FollowUserAsync(int userId, int userIdToFollow)
        {
            using var activity = LoggingService.activitySource.StartActivity();
            await userProfileRepository.FollowUserAsync(userId, userIdToFollow);
        }

        public async Task UnfollowUserAsync(int userId, int userIdToUnfollow)
        {
            using var activity = LoggingService.activitySource.StartActivity();
            await userProfileRepository.UnfollowUserAsync(userId, userIdToUnfollow);
        }

        // New method to get followers
        public async Task<List<int>> GetFollowersAsync(int userId)
        {
            using var activity = LoggingService.activitySource.StartActivity();
            return await userProfileRepository.GetFollowersAsync(userId);
        }

        public async Task CreateDb()
        {
            await userProfileRepository.RecreateDatabase();
        }
    }
}