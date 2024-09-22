using InteractionService.Dtos;

namespace InteractionService.Services
{
    public interface IInteractionService
    {
        Task LikeTweetAsync(LikeDto likeDto);
        Task CommentOnTweetAsync(CommentDto commentDto);
        Task<List<CommentDto>> GetCommentsForTweetAsync(int tweetId);
    }
}