namespace Shared.Events
{
    public class TweetEvent
    {
        public Guid UserId { get; set; }
        public Guid TweetId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty; // E.g., "TweetLiked", "TweetCommented"
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}