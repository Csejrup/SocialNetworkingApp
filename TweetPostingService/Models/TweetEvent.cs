namespace TweetPostingService.Models
{
    public class TweetEvent
    {
        public Guid UserId { get; set; }
        public Guid TweetId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } = string.Empty; // e.g., "TweetPosted" or "TweetDeleted"
    }
}