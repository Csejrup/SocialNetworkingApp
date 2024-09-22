using Microsoft.EntityFrameworkCore;
using TweetPostingService.Models;

namespace TweetPostingService.Data
{
    public class TweetDbContext(DbContextOptions<TweetDbContext> options) : DbContext(options)
    {
        public DbSet<Tweet> Tweets { get; set; }
    }
}