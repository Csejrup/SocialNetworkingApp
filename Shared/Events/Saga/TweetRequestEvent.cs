namespace Shared.Events.Saga;

public class TweetRequestEvent : BaseEvent
{
    public Guid UserId { get; set; } 
}