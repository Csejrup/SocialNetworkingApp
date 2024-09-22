namespace InteractionService.Models
{
    public class Like
    {
        public int Id { get; set; }
        public int UserId { get; set; } // The user who liked the tweet
        public int TweetId { get; set; } // The liked tweet
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}