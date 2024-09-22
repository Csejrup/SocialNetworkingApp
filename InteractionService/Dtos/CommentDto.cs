namespace InteractionService.Dtos
{
    public class CommentDto
    {
        public int UserId { get; set; }
        public int TweetId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}