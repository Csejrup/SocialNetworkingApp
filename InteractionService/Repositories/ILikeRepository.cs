using InteractionService.Models;

namespace InteractionService.Repositories
{
    public interface ILikeRepository
    {
        Task AddLikeAsync(Like like);
    }
}