using Shared.Events;
using Shared.Messaging;
using TweetPostingService.Dtos;
using TweetPostingService.Models;
using TweetPostingService.Repositories;


namespace TweetPostingService.Services
{
    public class TweetService(ITweetRepository tweetRepository, MessageClient messageClient)
        : ITweetService
    {
        public async Task PostTweetAsync(TweetDto tweetDto)
        {
            
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

            messageClient.Send(tweetEvent, "TweetPosted");
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

            messageClient.Send(tweetEvent, "TweetDeleted");
        }
    }
}
