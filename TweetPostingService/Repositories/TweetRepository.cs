using Microsoft.EntityFrameworkCore;
using TweetPostingService.Models;
using TweetPostingService.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Monitoring;

namespace TweetPostingService.Repositories
{
    public class TweetRepository(TweetDbContext context) : ITweetRepository
    {
        public async Task AddTweetAsync(Tweet tweet)
        {
            using var _ = LoggingService.activitySource.StartActivity();

            await context.Tweets.AddAsync(tweet);
            await context.SaveChangesAsync();
        }

        public async Task<Tweet?> GetTweetByIdAsync(int tweetId)
        {
            using var _ = LoggingService.activitySource.StartActivity();

            return await context.Tweets.FindAsync(tweetId);
        }

        public async Task DeleteTweetAsync(Tweet tweet)
        {
            using var _ = LoggingService.activitySource.StartActivity();

            context.Tweets.Remove(tweet);
            await context.SaveChangesAsync();
        }

        public async Task<List<Tweet>> GetTweetsByUserIdAsync(int userId)
        {
            using var _ = LoggingService.activitySource.StartActivity();

            return await context.Tweets.Where(t => t.UserId == userId).ToListAsync();
        }
    }
}