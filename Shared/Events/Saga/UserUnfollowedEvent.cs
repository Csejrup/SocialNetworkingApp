namespace Shared.Events.Saga;

public class UserUnfollowedEvent : BaseEvent
{
    public Guid UserId { get; set; }
    public Guid UnfollowedUserId { get; set; }

}