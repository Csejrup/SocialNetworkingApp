using Shared.Dtos;

namespace Shared.Events.Saga;

public class TweetResponseEvent : BaseEvent
{
    public Guid UserId { get; set; } 
    public List<TweetDto> Tweets { get; set; } = new(); 
}