namespace InteractionService.Dtos
{
    public class CommentDto
    {
        public Guid UserId { get; set; }
        public Guid TweetId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}