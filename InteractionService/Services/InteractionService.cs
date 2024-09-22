using InteractionService.Dtos;
using InteractionService.Models;
using InteractionService.Repositories;

namespace InteractionService.Services
{
    public class InteractionService : IInteractionService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly HttpClient _httpClient;

        public InteractionService(ILikeRepository likeRepository, ICommentRepository commentRepository, HttpClient httpClient)
        {
            _likeRepository = likeRepository;
            _commentRepository = commentRepository;
            _httpClient = httpClient;
        }

        public async Task LikeTweetAsync(LikeDto likeDto)
        {
            // Call the TweetPostingService to check if the tweet exists
            var tweetExists = await _httpClient.GetFromJsonAsync<bool>($"http://tweetpostingservice/api/tweet/{likeDto.TweetId}");

            if (!tweetExists)
            {
                throw new Exception($"Tweet with ID {likeDto.TweetId} does not exist.");
            }

            var like = new Like
            {
                UserId = likeDto.UserId,
                TweetId = likeDto.TweetId,
                LikedAt = DateTime.UtcNow
            };

            await _likeRepository.AddLikeAsync(like);
        }

        public async Task CommentOnTweetAsync(CommentDto commentDto)
        {
            // Call the TweetPostingService to check if the tweet exists
            var tweetExists = await _httpClient.GetFromJsonAsync<bool>($"http://tweetpostingservice/api/tweet/{commentDto.TweetId}");

            if (!tweetExists)
            {
                throw new Exception($"Tweet with ID {commentDto.TweetId} does not exist.");
            }

            var comment = new Comment
            {
                UserId = commentDto.UserId,
                TweetId = commentDto.TweetId,
                Content = commentDto.Content,
                CommentedAt = DateTime.UtcNow
            };

            await _commentRepository.AddCommentAsync(comment);
        }

        public async Task<List<CommentDto>> GetCommentsForTweetAsync(int tweetId)
        {
            var comments = await _commentRepository.GetCommentsByTweetIdAsync(tweetId);

            return comments.Select(c => new CommentDto
            {
                UserId = c.UserId,
                TweetId = c.TweetId,
                Content = c.Content
            }).ToList();
        }
    }
}
