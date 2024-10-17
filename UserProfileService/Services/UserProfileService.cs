using Shared.Messaging;
using UserProfileService.Dtos;
using UserProfileService.Models;
using UserProfileService.Repositories;

namespace UserProfileService.Services
{
    public class UserProfileService(IUserProfileRepository userProfileRepository, MessageClient messageClient) : IUserProfileService
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

                // After registering the user, send a message to other services
                var message = new UserProfileUpdatedMessage
                {
                    UserId = userProfile.Id,
                    Name = userProfile.Username,
                    Email = userProfile.Email
                };

                messageClient.Send(message, "UserProfileUpdatedMessage");
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

        public async Task<List<Guid>> GetFollowersAsync(Guid userId)
        {
            return await userProfileRepository.GetFollowersAsync(userId);
        }
    }
}