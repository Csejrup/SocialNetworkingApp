namespace Shared.Events.Saga;

public class UserFollowedEvent : BaseEvent
{
    public Guid UserId { get; set; }
    public Guid FollowedUserId { get; set; }

}