using InteractionService.Models;
using InteractionService.Data;

namespace InteractionService.Repositories
{
    public class LikeRepository(InteractionDbContext context) : ILikeRepository
    {
        public async Task AddLikeAsync(Like like)
        {
            await context.Likes.AddAsync(like);
            await context.SaveChangesAsync();
        }
    }
}