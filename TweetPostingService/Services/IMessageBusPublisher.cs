using TweetPostingService.Models;

namespace TweetPostingService.Services
{
    public interface IMessageBusPublisher
    {
        void PublishTweetEvent(TweetEvent tweetEvent);
    }
}
