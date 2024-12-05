namespace Shared.Events.Saga;

public class UserProfileUpdatedEvent : BaseEvent
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
}