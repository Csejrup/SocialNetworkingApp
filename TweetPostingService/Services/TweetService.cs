using Shared.Dtos;
using Shared.Events;
using Shared.Messaging;
using TweetPostingService.Models;
using TweetPostingService.Repositories;

namespace TweetPostingService.Services
{
    public class TweetService : ITweetService
    {
        private readonly ITweetRepository _tweetRepository;
        private readonly MessageClient _messageClient;

        public TweetService(ITweetRepository tweetRepository, MessageClient messageClient)
        {
            _tweetRepository = tweetRepository;
            _messageClient = messageClient;

            // Listen for requests to fetch tweets for a user
            _messageClient.Listen<TweetRequestMessage>(HandleTweetRequest, "RequestTweetsForUser");
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
            _messageClient.Send(response, "UserTweetsFetched");
        }

        public async Task PostTweetAsync(TweetDto tweetDto)
        {
            var tweet = new Tweet
            {
                UserId = tweetDto.UserId,
                Content = tweetDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _tweetRepository.AddTweetAsync(tweet);

            // Publish the tweet event to RabbitMQ
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
            var tweet = await _tweetRepository.GetTweetByIdAsync(tweetId);
            if (tweet == null || tweet.UserId != userId)
            {
                throw new Exception("You can only delete your own tweets.");
            }

            await _tweetRepository.DeleteTweetAsync(tweet);

            // Publish the tweet deleted event to RabbitMQ
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
