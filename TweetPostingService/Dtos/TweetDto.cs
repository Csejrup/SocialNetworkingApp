namespace TweetPostingService.Dtos
{
    public class TweetDto
    {
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}