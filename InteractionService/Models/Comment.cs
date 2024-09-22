namespace InteractionService.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int UserId { get; set; } // The user who commented
        public int TweetId { get; set; } // The commented tweet
        public string Content { get; set; } = string.Empty;
        public DateTime CommentedAt { get; set; } = DateTime.UtcNow;
    }
}