using TweetPostingService.Dtos;
using TweetPostingService.Models;
using TweetPostingService.Repositories;
using UserProfileService.Dtos;

namespace TweetPostingService.Services
{
    public class TweetService(
        ITweetRepository tweetRepository,
        HttpClient httpClient,
        IMessageBusPublisher messageBusPublisher)
        : ITweetService
    {
        public async Task PostTweetAsync(TweetDto tweetDto)
        {
            // Fetch user info from UserProfileService
            var user = await httpClient.GetFromJsonAsync<UserProfileDto>($"http://userprofileservice/api/userprofile/{tweetDto.UserId}");

            if (user == null)
            {
                throw new Exception($"User with ID {tweetDto.UserId} does not exist.");
            }

            var tweet = new Tweet
            {
                UserId = tweetDto.UserId,
                Content = tweetDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            await tweetRepository.AddTweetAsync(tweet);

            // Publish the tweet event to RabbitMQ
            var tweetEvent = new TweetEvent
            {
                UserId = tweetDto.UserId,
                TweetId = tweet.Id,
                Content = tweet.Content,
                EventType = "TweetPosted"
            };

            messageBusPublisher.PublishTweetEvent(tweetEvent);
        }

        public async Task<List<TweetDto>> GetTweetsByUserAsync(Guid userId)
        {
            // Get all tweets from a specific user
            var tweets = await tweetRepository.GetTweetsByUserIdAsync(userId);

            return tweets.Select(t => new TweetDto
            {
                UserId = t.UserId,
                Content = t.Content
            }).ToList();
        }

        public async Task DeleteTweetAsync(Guid tweetId, Guid userId)
        {
            var tweet = await tweetRepository.GetTweetByIdAsync(tweetId);
            if (tweet == null || tweet.UserId != userId)
            {
                throw new Exception("You can only delete your own tweets.");
            }

            await tweetRepository.DeleteTweetAsync(tweet);

            // Publish the tweet deleted event to RabbitMQ
            var tweetEvent = new TweetEvent
            {
                UserId = tweet.UserId,
                TweetId = tweet.Id,
                Content = tweet.Content,
                EventType = "TweetDeleted"
            };

            messageBusPublisher.PublishTweetEvent(tweetEvent);
        }
    }
}
