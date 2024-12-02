using Shared.Dtos;
using Shared.Events;
using Shared.Events.Saga;
using Shared.Messaging;
using TweetPostingService.Models;
using TweetPostingService.Repositories;
using UserProfileService.Models;

namespace TweetPostingService.Services;

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
        try
        {
            await _userCacheRepository.UpdateUserProfileAsync(new UserProfileCache
            {
                UserId = message.UserId,
                Name = message.Name,
                Email = message.Email
            });

            Console.WriteLine($"User profile updated for User ID: {message.UserId}");
        }
        catch (Exception ex)
        {
            // Publish a failure event if the cache update fails
            var failureEvent = new UserProfileUpdateFailedEvent
            {
                UserId = message.UserId,
                Reason = ex.Message
            };
            _messageClient.Send(failureEvent, "UserProfileUpdateFailed");
        }
    }

    private async void HandleTweetRequest(TweetRequestMessage request)
    {
        var tweets = await _tweetRepository.GetTweetsByUserIdAsync(request.UserId);

        var response = new TweetResponseEvent
        {
            UserId = request.UserId,
            Tweets = tweets.Select(t => new TweetDto
            {
                TweetId = t.Id,
                Content = t.Content,
                CreatedAt = t.CreatedAt
            }).ToList()
        };

        // Send the response message back 
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

        // Create a new Tweet 
        var tweet = new Tweet
        {
            UserId = tweetDto.UserId,
            Content = tweetDto.Content,
            CreatedAt = DateTime.UtcNow
        };

        // Save the tweet
        await _tweetRepository.AddTweetAsync(tweet);
        try
        {

            // Publish the TweetPosted event
            
            var tweetPostedEvent = new TweetPostedEvent
            {
                TweetId = tweet.Id,
                UserId = tweet.UserId,
                Content = tweet.Content
            };
            _messageClient.Send(tweetPostedEvent, "TweetPosted");
        }
        catch (Exception ex)
        {
            var failureEvent = new TweetPostFailedEvent
            {
                TweetId = tweet.Id,
                UserId = tweet.UserId,
                Reason = ex.Message
            };
            _messageClient.Send(failureEvent, "TweetPostFailed");
        }

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
        try
        {
            // Fetch the tweet
            var tweet = await _tweetRepository.GetTweetByIdAsync(tweetId);
            if (tweet == null || tweet.UserId != userId)
            {
                throw new Exception("You can only delete your own tweets.");
            }

            // Delete the tweet
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
        catch (Exception ex)
        {
            // Publish a compensating event if deletion fails
            var failureEvent = new TweetDeleteFailedEvent
            {
                TweetId = tweetId,
                UserId = userId,
                Reason = ex.Message
            };
            _messageClient.Send(failureEvent, "TweetDeleteFailed");
        }
    }


}