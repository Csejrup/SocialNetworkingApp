namespace InteractionService.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } // The user who commented
        public Guid TweetId { get; set; } // The commented tweet
        public string Content { get; set; } = string.Empty;
        public DateTime CommentedAt { get; set; } = DateTime.UtcNow;
    }
}