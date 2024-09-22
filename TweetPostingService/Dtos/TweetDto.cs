namespace TweetPostingService.Dtos
{
    public class TweetDto
    {
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}