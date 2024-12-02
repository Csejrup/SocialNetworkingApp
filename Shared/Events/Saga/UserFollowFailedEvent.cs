namespace Shared.Events.Saga;

public class UserFollowFailedEvent : BaseEvent
{
    public Guid UserId { get; set; }
    public Guid FollowedUserId { get; set; }
    public string Reason { get; set; }
}