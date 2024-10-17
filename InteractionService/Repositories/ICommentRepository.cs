using InteractionService.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace InteractionService.Repositories
{
    public interface ICommentRepository
    {
        Task AddCommentAsync(Comment comment);
        Task<List<Comment>> GetCommentsByTweetIdAsync(Guid tweetId);
    }
}