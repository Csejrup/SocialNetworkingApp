using TweetPostingService.Dtos;

namespace TweetPostingService.Services
{
    public interface ITweetService
    {
        Task PostTweetAsync(TweetDto tweetDto);
        Task<List<TweetDto>> GetTweetsByUserAsync(Guid userId);
        Task DeleteTweetAsync(Guid tweetId, Guid userId);
    }
}