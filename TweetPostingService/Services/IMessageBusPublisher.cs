using System.Diagnostics;
using TweetPostingService.Models;

namespace TweetPostingService.Services
{
    public interface IMessageBusPublisher
    {
        void PublishTweetEvent(TweetEvent tweetEvent);
        void PublishTweetEventWithActivityContext(TweetEvent tweetEvent, Activity activity);

    }
}