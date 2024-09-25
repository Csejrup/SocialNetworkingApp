using System.Text.Json;
using Monitoring;
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
            using var _ = LoggingService.activitySource.StartActivity();
            UserProfileDto? user = null;
            
            using (var activity = LoggingService.activitySource.StartActivity())
            {
                
                // Fetch user info from UserProfileService
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"http://userprofileservice/api/userprofile/{tweetDto.UserId}");
                request = LoggingService.AddActivityInfoToHttpRequest(request, activity);

                var res = await httpClient.SendAsync(request);
                var content = res.Content.ReadAsStringAsync().Result;
                user = JsonSerializer.Deserialize<UserProfileDto>(content);
                
                if (user == null)
                {
                    throw new Exception($"User with ID {tweetDto.UserId} does not exist.");
                }
            }
            

            var tweet = new Tweet
            {
                UserId = tweetDto.UserId,
                Content = tweetDto.Content,
                CreatedAt = DateTime.UtcNow
            };
            
            using (var activityTweet = LoggingService.activitySource.StartActivity())
            {
                await tweetRepository.AddTweetAsync(tweet);
            }


            using (var msBusActivity = LoggingService.activitySource.StartActivity())
            {
                
                // Publish the tweet event to RabbitMQ
                var tweetEvent = new TweetEvent
                {
                    UserId = tweetDto.UserId,
                    TweetId = tweet.Id,
                    Content = tweet.Content,
                    EventType = "TweetPosted"
                };

                messageBusPublisher.PublishTweetEventWithActivityContext(tweetEvent, msBusActivity);
                
            }

       
        }

        public async Task<List<TweetDto>> GetTweetsByUserAsync(int userId)
        {
            using var _ = LoggingService.activitySource.StartActivity();
            // Get all tweets from a specific user
            var tweets = await tweetRepository.GetTweetsByUserIdAsync(userId);

            return tweets.Select(t => new TweetDto
            {
                UserId = t.UserId,
                Content = t.Content
            }).ToList();
        }

        public async Task DeleteTweetAsync(int tweetId, int userId)
        {
            using var _ = LoggingService.activitySource.StartActivity();

            var tweet = await tweetRepository.GetTweetByIdAsync(tweetId);
            if (tweet == null || tweet.UserId != userId)
            {
                throw new Exception("You can only delete your own tweets.");
            }

            await tweetRepository.DeleteTweetAsync(tweet);


            using (var activity = LoggingService.activitySource.StartActivity())
            {
                // Publish the tweet deleted event to RabbitMQ
                var tweetEvent = new TweetEvent
                {
                    UserId = tweet.UserId,
                    TweetId = tweet.Id,
                    Content = tweet.Content,
                    EventType = "TweetDeleted"
                };
                messageBusPublisher.PublishTweetEventWithActivityContext(tweetEvent, activity);

            }

        }
    }
}
