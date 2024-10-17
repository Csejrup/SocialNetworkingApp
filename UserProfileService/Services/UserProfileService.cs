using Shared.Dtos;
using Shared.Messaging;
using UserProfileService.Dtos;
using UserProfileService.Models;
using UserProfileService.Repositories;

namespace UserProfileService.Services
{
    public class UserProfileService(IUserProfileRepository userProfileRepository, MessageClient messageClient)
        : IUserProfileService
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

            var message = new UserProfileUpdatedMessage
            {
                UserId = userProfile.Id,
                Name = userProfile.Username,
                Email = userProfile.Email
            };

            messageClient.Send(message, "UserProfileUpdatedMessage");
        }

        public async Task<UserProfileWithTweetsDto?> GetUserProfileWithTweetsAsync(Guid userId)
        {
            // Get the user's profile
            var userProfile = await userProfileRepository.GetUserProfileByIdAsync(userId);
            if (userProfile == null) return null;

            // Send a message to RabbitMQ requesting tweets for the user
            var tweetRequest = new TweetRequestMessage
            {
                UserId = userId
            };
            messageClient.Send(tweetRequest, "RequestTweetsForUser");

            var tweets = await ListenForTweetsResponse(userId, maxRetries: 3, timeoutInMilliseconds: 3000);

            if (tweets == null)
            {
                return new UserProfileWithTweetsDto
                {
                    UserId = userProfile.Id,
                    Username = userProfile.Username,
                    Bio = userProfile.Bio,
                    Tweets = new List<TweetDto>()
                };
            }

            return new UserProfileWithTweetsDto
            {
                UserId = userProfile.Id,
                Username = userProfile.Username,
                Bio = userProfile.Bio,
                Tweets = tweets
            };
        }

        private async Task<List<TweetDto>?> ListenForTweetsResponse(Guid userId, int maxRetries, int timeoutInMilliseconds)
        {
            var retryCount = 0;
            List<TweetDto>? tweets = null;

            while (retryCount < maxRetries && tweets == null)
            {
                try
                {
                    using (var cts = new CancellationTokenSource(timeoutInMilliseconds))
                    {
                        tweets = await ListenForTweetsResponse(userId, cts.Token);
                    }

                    if (tweets != null)
                    {
                        return tweets; 
                    }
                }
                catch (TaskCanceledException)
                {
                    // Handle timeout by retrying
                    retryCount++;
                    Console.WriteLine($"Retry {retryCount}/{maxRetries} - No response received within timeout.");
                }
            }

            Console.WriteLine("Failed to retrieve tweets after maximum retries.");
            return null; 
        }

        private Task<List<TweetDto>?> ListenForTweetsResponse(Guid userId, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<List<TweetDto>?>();
            List<TweetDto> tweets = [];

            // Subscribe to the "UserTweetsFetched" event with a handler
            messageClient.Listen<TweetResponseMessage>(response =>
            {
                if (response.UserId != userId) return;
                tweets.AddRange(response.Tweets);
                tcs.SetResult(tweets); 
            }, "UserTweetsFetched");

            cancellationToken.Register(() => tcs.TrySetCanceled());

            return tcs.Task;
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
