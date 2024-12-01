using Microsoft.EntityFrameworkCore;
using InteractionService.Models;
using InteractionService.Data;

namespace InteractionService.Repositories
{
    public class CommentRepository(InteractionDbContext context) : ICommentRepository
    {
        public async Task AddCommentAsync(Comment comment)
        {
            await context.Comments.AddAsync(comment);
            await context.SaveChangesAsync();
        }

        public async Task<List<Comment>> GetCommentsByTweetIdAsync(Guid tweetId)
        {
            return await context.Comments.Where(c => c.TweetId == tweetId).ToListAsync();
        }
    }
}