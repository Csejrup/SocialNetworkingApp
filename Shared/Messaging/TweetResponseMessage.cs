using Shared.Dtos;

namespace Shared.Messaging;

public class TweetResponseMessage
{
    public Guid UserId { get; set; }
    public List<TweetDto> Tweets { get; set; } = new ();
}