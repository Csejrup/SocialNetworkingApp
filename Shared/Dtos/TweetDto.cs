namespace Shared.Dtos;

public class TweetDto
{
    public Guid TweetId { get; set; } // The ID of the tweet
    public Guid UserId { get; set; }  // The ID of the user who posted the tweet
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}