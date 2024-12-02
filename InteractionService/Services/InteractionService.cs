using InteractionService.Dtos;
using InteractionService.Models;
using InteractionService.Repositories;
using Shared.Events;
using Shared.Events.Saga;
using Shared.Messaging;

namespace InteractionService.Services
{
    public class InteractionService(
        ILikeRepository likeRepository,
        ICommentRepository commentRepository,
        IMessageClient messageClient)
        : IInteractionService
    {
        public async Task LikeTweetAsync(LikeDto likeDto)
        {
            try
            {
                var like = new Like
                {
                    UserId = likeDto.UserId,
                    TweetId = likeDto.TweetId,
                    LikedAt = DateTime.UtcNow
                };

                await likeRepository.AddLikeAsync(like);

                // Publish a "TweetLiked" event
                var tweetEvent = new TweetEvent
                {
                    UserId = likeDto.UserId,
                    TweetId = likeDto.TweetId,
                    EventType = "TweetLiked"
                };

                messageClient.Send(tweetEvent, "TweetLiked");
            }
            catch (Exception ex)
            {
                // Publish a compensating event
                var failureEvent = new TweetLikeFailedEvent
                {
                    UserId = likeDto.UserId,
                    TweetId = likeDto.TweetId,
                    Reason = ex.Message
                };

                messageClient.Send(failureEvent, "TweetLikeFailed");
                throw; // Re-throw exception
            }
        }


        public async Task CommentOnTweetAsync(CommentDto commentDto)
        {
            try
            {
                var comment = new Comment
                {
                    UserId = commentDto.UserId,
                    TweetId = commentDto.TweetId,
                    Content = commentDto.Content,
                    CommentedAt = DateTime.UtcNow
                };

                await commentRepository.AddCommentAsync(comment);

                // Publish a "TweetCommented"
                var tweetEvent = new TweetEvent
                {
                    UserId = commentDto.UserId,
                    TweetId = commentDto.TweetId,
                    Content = commentDto.Content,
                    EventType = "TweetCommented"
                };

                messageClient.Send(tweetEvent, "TweetCommented");
            }
            catch (Exception ex)
            {
                // Publish a compensating event
                var failureEvent = new TweetCommentFailedEvent
                {
                    UserId = commentDto.UserId,
                    TweetId = commentDto.TweetId,
                    Content = commentDto.Content,
                    Reason = ex.Message
                };

                messageClient.Send(failureEvent, "TweetCommentFailed");
                throw;
            }
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
