namespace Shared.Events.Saga;

public class UserProfileRegistrationFailedEvent : BaseEvent
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Reason { get; set; }
}