using InteractionService.Dtos;
using InteractionService.Models;
using InteractionService.Repositories;
using Shared.Events;
using Shared.Messaging;

namespace InteractionService.Services
{
    public class InteractionService(
        ILikeRepository likeRepository,
        ICommentRepository commentRepository,
        MessageClient messageClient)
        : IInteractionService
    {
        public async Task LikeTweetAsync(LikeDto likeDto)
        {
            
            var like = new Like
            {
                UserId = likeDto.UserId,
                TweetId = likeDto.TweetId,
                LikedAt = DateTime.UtcNow
            };

            await likeRepository.AddLikeAsync(like);

            // Publish a "TweetLiked" event to RabbitMQ
            var tweetEvent = new TweetEvent
            {
                UserId = likeDto.UserId,
                TweetId = likeDto.TweetId,
                EventType = "TweetLiked"
            };

            messageClient.Send(tweetEvent, "TweetLiked");
        }

        public async Task CommentOnTweetAsync(CommentDto commentDto)
        {
            var comment = new Comment
            {
                UserId = commentDto.UserId,
                TweetId = commentDto.TweetId,
                Content = commentDto.Content,
                CommentedAt = DateTime.UtcNow
            };

            await commentRepository.AddCommentAsync(comment);

            // Publish a "TweetCommented" event to RabbitMQ
            var tweetEvent = new TweetEvent
            {
                UserId = commentDto.UserId,
                TweetId = commentDto.TweetId,
                Content = commentDto.Content,
                EventType = "TweetCommented"
            };

            messageClient.Send(tweetEvent, "TweetCommented");
        }

        public async Task<List<CommentDto>> GetCommentsForTweetAsync(Guid tweetId)
        {
            var comments = await commentRepository.GetCommentsByTweetIdAsync(tweetId);

            return comments.Select(c => new CommentDto
            {
                UserId = c.UserId,
                TweetId = c.TweetId,
                Content = c.Content
            }).ToList();
        }
    }
}
