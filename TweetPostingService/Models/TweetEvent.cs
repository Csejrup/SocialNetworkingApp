namespace TweetPostingService.Models
{
    public class TweetEvent
    {
        public int UserId { get; set; }
        public int TweetId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } = string.Empty; // e.g., "TweetPosted" or "TweetDeleted"
    }
}