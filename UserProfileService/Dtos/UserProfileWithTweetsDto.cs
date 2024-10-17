using Shared.Dtos;

namespace UserProfileService.Dtos;

public class UserProfileWithTweetsDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public List<TweetDto> Tweets { get; set; } = new List<TweetDto>();
}