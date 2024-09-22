using TweetPostingService.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TweetPostingService.Repositories
{
    public interface ITweetRepository
    {
        Task AddTweetAsync(Tweet tweet);
        Task<Tweet?> GetTweetByIdAsync(int tweetId);
        Task DeleteTweetAsync(Tweet tweet);
        Task<List<Tweet>> GetTweetsByUserIdAsync(int userId);
    }
}