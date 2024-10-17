using Microsoft.EntityFrameworkCore;
using TweetPostingService.Models;
using TweetPostingService.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TweetPostingService.Repositories
{
    public class TweetRepository(TweetDbContext context) : ITweetRepository
    {
        public async Task AddTweetAsync(Tweet tweet)
        {
            await context.Tweets.AddAsync(tweet);
            await context.SaveChangesAsync();
        }

        public async Task<Tweet?> GetTweetByIdAsync(Guid tweetId)
        {
            return await context.Tweets.FindAsync(tweetId);
        }

        public async Task DeleteTweetAsync(Tweet tweet)
        {
            context.Tweets.Remove(tweet);
            await context.SaveChangesAsync();
        }

        public async Task<List<Tweet>> GetTweetsByUserIdAsync(Guid userId)
        {
            return await context.Tweets.Where(t => t.UserId == userId).ToListAsync();
        }
    }
}