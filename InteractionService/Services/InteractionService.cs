using InteractionService.Dtos;
using InteractionService.Models;
using InteractionService.Repositories;
using Shared.Events;
using Shared.Messaging;
using Polly;
using Polly.CircuitBreaker;


namespace InteractionService.Services
{
    public class InteractionService : IInteractionService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IMessageClient _messageClient;
        private readonly ILogger<InteractionService> _logger;
        private static readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        static InteractionService()
        {
            
            _circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    3, 
                    TimeSpan.FromSeconds(30), 
                    onBreak: (exception, timespan) =>
                    {
                      
                        Console.WriteLine($"Circuit breaker triggered due to error: {exception.Message}. Circuit open for {timespan.TotalSeconds} seconds.");
                    },
                    onReset: () =>
                    {
                       
                        Console.WriteLine("Circuit breaker reset, retrying operations.");
                    });
        }

        // Constructor
        public InteractionService(
            ILikeRepository likeRepository,
            ICommentRepository commentRepository,
            IMessageClient messageClient,
            ILogger<InteractionService> logger)
        {
            _likeRepository = likeRepository;
            _commentRepository = commentRepository;
            _messageClient = messageClient;
            _logger = logger;
        }

        // Method to like a tweet
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

                await _likeRepository.AddLikeAsync(like);

                var tweetEvent = new TweetEvent
                {
                    UserId = likeDto.UserId,
                    TweetId = likeDto.TweetId,
                    EventType = "TweetLiked"
                };

             
                await PublishTweetEventAsync(tweetEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while liking the tweet.");
                throw new InvalidOperationException("Failed to like the tweet.", ex);
            }
        }

        // Method to comment on a tweet
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

                await _commentRepository.AddCommentAsync(comment);

                var tweetEvent = new TweetEvent
                {
                    UserId = commentDto.UserId,
                    TweetId = commentDto.TweetId,
                    Content = commentDto.Content,
                    EventType = "TweetCommented"
                };

                
                await PublishTweetEventAsync(tweetEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while commenting on the tweet.");
                throw new InvalidOperationException("Failed to comment on the tweet.", ex);
            }
        }

        // Helper method to publish events to RabbitMQ
        private async Task PublishTweetEventAsync(TweetEvent tweetEvent)
        {
            try
            {
                
                await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    await Task.Run(() => _messageClient.Send(tweetEvent, tweetEvent.EventType));
                });
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogWarning($"Circuit breaker is open. Event not sent: {ex.Message}");
                throw new InvalidOperationException("Circuit breaker open. Event not sent to RabbitMQ.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while publishing event to RabbitMQ.");
                throw new InvalidOperationException("Failed to publish event to RabbitMQ.", ex);
            }
        }

        // Method for getting comments for a tweet
        public async Task<List<CommentDto>> GetCommentsForTweetAsync(Guid tweetId)
        {
            try
            {
                var comments = await _commentRepository.GetCommentsByTweetIdAsync(tweetId);

                if (comments == null || !comments.Any())
                {
                    _logger.LogWarning($"No comments found for tweet with ID {tweetId}.");
                    return new List<CommentDto>(); // Return empty list if no comments found
                }

                return comments.Select(c => new CommentDto
                {
                    UserId = c.UserId,
                    TweetId = c.TweetId,
                    Content = c.Content
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while fetching comments for tweet with ID {tweetId}.");
                throw new InvalidOperationException("Failed to fetch comments for tweet.", ex);
            }
        }
    }
}
