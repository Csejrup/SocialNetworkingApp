namespace Shared.Events.Saga;

public class TweetPostedEvent : BaseEvent
{
    public Guid TweetId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; }
}