using InteractionService.Dtos;
using InteractionService.Models;
using InteractionService.Repositories;
using Shared.Events;
using Shared.Messaging;

// Assuming the TweetPostingService has models like TweetEvent

namespace InteractionService.Services
{
    public class InteractionService : IInteractionService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly MessageClient _messageClient;

        public InteractionService(ILikeRepository likeRepository, ICommentRepository commentRepository, MessageClient messageClient)
        {
            _likeRepository = likeRepository;
            _commentRepository = commentRepository;
            _messageClient = messageClient;
        }

        public async Task LikeTweetAsync(LikeDto likeDto)
        {
            // Simulate fetching tweet information using RabbitMQ (skipping HTTP calls)
            // For the sake of this example, we assume the tweet exists

            var like = new Like
            {
                UserId = likeDto.UserId,
                TweetId = likeDto.TweetId,
                LikedAt = DateTime.UtcNow
            };

            await _likeRepository.AddLikeAsync(like);

            // Publish a "TweetLiked" event to RabbitMQ
            var tweetEvent = new TweetEvent
            {
                UserId = likeDto.UserId,
                TweetId = likeDto.TweetId,
                EventType = "TweetLiked"
            };

            _messageClient.Send(tweetEvent, "TweetLiked");
        }

        public async Task CommentOnTweetAsync(CommentDto commentDto)
        {
            // Simulate fetching tweet information using RabbitMQ (skipping HTTP calls)
            // For the sake of this example, we assume the tweet exists

            var comment = new Comment
            {
                UserId = commentDto.UserId,
                TweetId = commentDto.TweetId,
                Content = commentDto.Content,
                CommentedAt = DateTime.UtcNow
            };

            await _commentRepository.AddCommentAsync(comment);

            // Publish a "TweetCommented" event to RabbitMQ
            var tweetEvent = new TweetEvent
            {
                UserId = commentDto.UserId,
                TweetId = commentDto.TweetId,
                Content = commentDto.Content,
                EventType = "TweetCommented"
            };

            _messageClient.Send(tweetEvent, "TweetCommented");
        }

        public async Task<List<CommentDto>> GetCommentsForTweetAsync(Guid tweetId)
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
