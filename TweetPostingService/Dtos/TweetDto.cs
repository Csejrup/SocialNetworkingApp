namespace TweetPostingService.Dtos
{
    public class TweetDto
    {
        
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}