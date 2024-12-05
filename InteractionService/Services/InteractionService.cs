using InteractionService.Dtos;
using InteractionService.Models;
using InteractionService.Repositories;
using Shared.Events;
using Shared.Messaging;
using Polly.Retry;
using Shared.Events.Saga;
using Shared.Utils;

namespace InteractionService.Services
{
    public class InteractionService : IInteractionService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IMessageClient _messageClient;
        private readonly AsyncRetryPolicy _retryPolicy;

        public InteractionService(
            ILikeRepository likeRepository,
            ICommentRepository commentRepository,
            IMessageClient messageClient)
        {
            _likeRepository = likeRepository;
            _commentRepository = commentRepository;
            _messageClient = messageClient;

            // Initialize retry policy
            _retryPolicy = PollyRetryPolicy.CreateRetryPolicy();
        }

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

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await Task.Run(() => _likeRepository.AddLikeAsync(like));
                });

                // Publish a "TweetLiked" event
                var tweetEvent = new TweetEvent
                {
                    UserId = likeDto.UserId,
                    TweetId = likeDto.TweetId,
                    EventType = "TweetLiked"
                };

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await Task.Run(() => _messageClient.Send(tweetEvent, "TweetLiked")); 
                });
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

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await Task.Run(() => _messageClient.Send(failureEvent, "TweetLikeFailed")); 
                });
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

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await Task.Run(() => _commentRepository.AddCommentAsync(comment)); 
                });

                // Publish a "TweetCommented" event
                var tweetEvent = new TweetEvent
                {
                    UserId = commentDto.UserId,
                    TweetId = commentDto.TweetId,
                    Content = commentDto.Content,
                    EventType = "TweetCommented"
                };

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await Task.Run(() => _messageClient.Send(tweetEvent, "TweetCommented")); 
                });
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

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await Task.Run(() => _messageClient.Send(failureEvent, "TweetCommentFailed")); 
                });
                throw;
            }
        }

        public async Task<List<CommentDto>> GetCommentsForTweetAsync(Guid tweetId)
        {
            var comments = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await Task.Run(() => _commentRepository.GetCommentsByTweetIdAsync(tweetId)); 
            });

            return comments.Select(c => new CommentDto
            {
                UserId = c.UserId,
                TweetId = c.TweetId,
                Content = c.Content
            }).ToList();
        }
    }
}
