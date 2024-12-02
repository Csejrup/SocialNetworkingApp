namespace Shared.Events.Saga;

public class UserUnfollowFailedEvent : BaseEvent
{
    public Guid UserId { get; set; }
    public Guid UnfollowedUserId { get; set; }
    public string Reason { get; set; }
}