namespace Shared.Events.Saga;

public class TweetPostFailedEvent : BaseEvent
{
    public Guid TweetId { get; set; }
    public Guid UserId { get; set; }
    public string Reason { get; set; }
}