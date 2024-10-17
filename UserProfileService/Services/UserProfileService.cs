using UserProfileService.Dtos;
using UserProfileService.Models;
using UserProfileService.Repositories;

namespace UserProfileService.Services
{
    public class UserProfileService(IUserProfileRepository userProfileRepository) : IUserProfileService
    {
        public async Task RegisterUserAsync(UserProfileDto userProfileDto)
        {
            var userProfile = new UserProfile
            {
                Username = userProfileDto.Username,
                Email = userProfileDto.Email,
                Bio = userProfileDto.Bio
            };

            await userProfileRepository.AddUserProfileAsync(userProfile);
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(Guid id)
        {
            var userProfile = await userProfileRepository.GetUserProfileByIdAsync(id);
            if (userProfile == null) return null;

            return new UserProfileDto
            {
                Username = userProfile.Username,
                Email = userProfile.Email,
                Bio = userProfile.Bio
            };
        }

        public async Task FollowUserAsync(Guid userId, Guid userIdToFollow)
        {
            await userProfileRepository.FollowUserAsync(userId, userIdToFollow);
        }

        public async Task UnfollowUserAsync(Guid userId, Guid userIdToUnfollow)
        {
            await userProfileRepository.UnfollowUserAsync(userId, userIdToUnfollow);
        }

        // New method to get followers
        public async Task<List<Guid>> GetFollowersAsync(Guid userId)
        {
            return await userProfileRepository.GetFollowersAsync(userId);
        }
    }
}