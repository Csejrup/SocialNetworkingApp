using TweetPostingService.Dtos;

namespace TweetPostingService.Services
{
    public interface ITweetService
    {
        Task PostTweetAsync(TweetDto tweetDto);
        Task<List<TweetDto>> GetTweetsByUserAsync(int userId);
        Task DeleteTweetAsync(int tweetId, int userId);
    }
}