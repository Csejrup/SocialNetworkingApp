using InteractionService.Models;

namespace InteractionService.Repositories
{
    public interface ICommentRepository
    {
        Task AddCommentAsync(Comment comment);
        Task<List<Comment>> GetCommentsByTweetIdAsync(Guid tweetId);
    }
}