namespace TweetPostingService.Models
{
    public class Tweet
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Reference to the user who posted the tweet
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}