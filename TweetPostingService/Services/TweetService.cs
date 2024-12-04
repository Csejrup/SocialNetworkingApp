using Shared.Dtos;
using Shared.Events;
using Shared.Messaging;
using TweetPostingService.Models;
using TweetPostingService.Repositories;
using UserProfileService.Models;

namespace TweetPostingService.Services
{
    public class TweetService : ITweetService
    {
        private readonly ITweetRepository _tweetRepository;
        private readonly IMessageClient _messageClient;
        private readonly IUserCacheRepository _userCacheRepository;
        public TweetService(ITweetRepository tweetRepository, IMessageClient messageClient, IUserCacheRepository userCacheRepository)
        {
            _tweetRepository = tweetRepository;
            _messageClient = messageClient;
            _userCacheRepository = userCacheRepository;

            // Listen for requests to fetch tweets for a user
            _messageClient.Listen<TweetRequestMessage>(HandleTweetRequest, "RequestTweetsForUser");
            _messageClient.Listen<UserProfileUpdatedMessage>(HandleUserProfileUpdated, "UserProfileUpdated");

        }
        private async void HandleUserProfileUpdated(UserProfileUpdatedMessage message)
        {
            await _userCacheRepository.UpdateUserProfileAsync(new UserProfileCache
            {
                UserId = message.UserId,
                Name = message.Name,
                Email = message.Email
            });

            Console.WriteLine($"User profile updated for User ID: {message.UserId}");
        }
        private async void HandleTweetRequest(TweetRequestMessage request)
        {
            var tweets = await _tweetRepository.GetTweetsByUserIdAsync(request.UserId);

            var response = new TweetResponseMessage
            {
                UserId = request.UserId,
                Tweets = tweets.Select(t => new TweetDto
                {
                    TweetId = t.Id,
                    Content = t.Content,
                    CreatedAt = t.CreatedAt
                }).ToList()
            };

            // Send the response message back via RabbitMQ to the UserProfileService

            //Curcuit breaker - errorhandling til hvis ikke tweet når ud 
            _messageClient.Send(response, "UserTweetsFetched");
        }

        public async Task PostTweetAsync(TweetDto tweetDto)
        {
            // Validate if the user exists in the local cache
            var userProfile = await _userCacheRepository.GetUserProfileAsync(tweetDto.UserId);
            if (userProfile == null)
            {
                throw new Exception("Invalid user: The user does not exist.");
            }

            // Create a new Tweet entity
            var tweet = new Tweet
            {
                UserId = tweetDto.UserId,
                Content = tweetDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            // Save the tweet to the repository
            await _tweetRepository.AddTweetAsync(tweet);

            // Publish the TweetPosted event
            var tweetEvent = new TweetEvent
            {
                UserId = tweetDto.UserId,
                TweetId = tweet.Id,
                Content = tweet.Content,
                EventType = "TweetPosted"
            };

            _messageClient.Send(tweetEvent, "TweetPosted");
        }


        public async Task<List<TweetDto>> GetTweetsByUserAsync(Guid userId)
        {
            var tweets = await _tweetRepository.GetTweetsByUserIdAsync(userId);

            return tweets.Select(t => new TweetDto
            {
                UserId = t.UserId,
                TweetId = t.Id,
                Content = t.Content,
                CreatedAt = t.CreatedAt
            }).ToList();
        }

        public async Task DeleteTweetAsync(Guid tweetId, Guid userId)
        {
            // Fetch the tweet by its ID
            var tweet = await _tweetRepository.GetTweetByIdAsync(tweetId);
            if (tweet == null || tweet.UserId != userId)
            {
                throw new Exception("You can only delete your own tweets.");
            }

            // Delete the tweet from the repository
            await _tweetRepository.DeleteTweetAsync(tweet);

            // Publish the TweetDeleted event
            var tweetEvent = new TweetEvent
            {
                UserId = tweet.UserId,
                TweetId = tweet.Id,
                Content = tweet.Content,
                EventType = "TweetDeleted"
            };

            _messageClient.Send(tweetEvent, "TweetDeleted");
        }

    }
}
