namespace InteractionService.Models
{
    public class Like
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } // The user who liked the tweet
        public Guid TweetId { get; set; } // The liked tweet
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}