using Shared.Dtos;
using Shared.Events;
using Shared.Events.Saga;
using Shared.Messaging;
using UserProfileService.Dtos;
using UserProfileService.Models;
using UserProfileService.Repositories;

namespace UserProfileService.Services
{
    public class UserProfileService(IUserProfileRepository userProfileRepository, IMessageClient messageClient)
        : IUserProfileService
    {
        public async Task<Guid> RegisterUserAsync(UserProfileDto userProfileDto)
        {
            try
            {
                var userProfile = new UserProfile
                {
                    Id = Guid.NewGuid(),
                    Username = userProfileDto.Username,
                    Email = userProfileDto.Email,
                    Bio = userProfileDto.Bio
                };

                await userProfileRepository.AddUserProfileAsync(userProfile);

                // Publish UserProfileUpdatedEvent
                var userProfileUpdatedEvent = new UserProfileUpdatedEvent
                {
                    UserId = userProfile.Id,
                    Username = userProfile.Username,
                    Email = userProfile.Email
                };

                messageClient.Send(userProfileUpdatedEvent, "UserProfileUpdated");
                return userProfile.Id;
            }
            catch (Exception ex)
            {
                // Publish compensating event
                var failureEvent = new UserProfileRegistrationFailedEvent
                {
                    Username = userProfileDto.Username,
                    Email = userProfileDto.Email,
                    Reason = ex.Message
                };

                messageClient.Send(failureEvent, "UserProfileRegistrationFailed");
                throw; // Re-throw exception
            }
        }


        public async Task<UserProfileWithTweetsDto?> GetUserProfileWithTweetsAsync(Guid userId)
        {
            // Get the user's profile
            var userProfile = await userProfileRepository.GetUserProfileByIdAsync(userId);
            if (userProfile == null) return null;

            // Send a message to request tweets for the user
            var tweetRequestEvent = new TweetRequestEvent
            {
                UserId = userId
            };

            messageClient.Send(tweetRequestEvent, "RequestTweetsForUser");

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
                        tweets = await ListenForTweetsResponseInternal(userId, cts.Token);
                    }

                    if (tweets != null)
                    {
                        return tweets;
                    }
                }
                catch (TaskCanceledException)
                {
                    retryCount++;
                    Console.WriteLine($"Retry {retryCount}/{maxRetries} - No response received within timeout.");
                }
            }

            Console.WriteLine("Failed to retrieve tweets after maximum retries.");
            return null;
        }

        private Task<List<TweetDto>?> ListenForTweetsResponseInternal(Guid userId, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<List<TweetDto>?>();
            var tweets = new List<TweetDto>();

            // Listen for the "UserTweetsFetched" event
            messageClient.Listen<TweetResponseEvent>(response =>
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
            try
            {
                await userProfileRepository.FollowUserAsync(userId, userIdToFollow);

                // Publish UserFollowedEvent
                var followEvent = new UserFollowedEvent
                {
                    UserId = userId,
                    FollowedUserId = userIdToFollow
                };

                messageClient.Send(followEvent, "UserFollowed");
            }
            catch (Exception ex)
            {
                // Publish compensating event
                var failureEvent = new UserFollowFailedEvent
                {
                    UserId = userId,
                    FollowedUserId = userIdToFollow,
                    Reason = ex.Message
                };

                messageClient.Send(failureEvent, "UserFollowFailed");
                throw;
            }
        }


        public async Task UnfollowUserAsync(Guid userId, Guid userIdToUnfollow)
        {
            try
            {
                await userProfileRepository.UnfollowUserAsync(userId, userIdToUnfollow);

                // Publish UserUnfollowedEvent
                var unfollowEvent = new UserUnfollowedEvent
                {
                    UserId = userId,
                    UnfollowedUserId = userIdToUnfollow
                };

                messageClient.Send(unfollowEvent, "UserUnfollowed");
            }
            catch (Exception ex)
            {
                // Publish compensating event
                var failureEvent = new UserUnfollowFailedEvent
                {
                    UserId = userId,
                    UnfollowedUserId = userIdToUnfollow,
                    Reason = ex.Message
                };

                messageClient.Send(failureEvent, "UserUnfollowFailed");
                throw;
            }
        }


        public async Task<List<Guid>> GetFollowersAsync(Guid userId)
        {
            return await userProfileRepository.GetFollowersAsync(userId);
        }
    }
}
