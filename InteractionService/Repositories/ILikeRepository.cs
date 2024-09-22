using InteractionService.Models;
using System.Threading.Tasks;

namespace InteractionService.Repositories
{
    public interface ILikeRepository
    {
        Task AddLikeAsync(Like like);
    }
}