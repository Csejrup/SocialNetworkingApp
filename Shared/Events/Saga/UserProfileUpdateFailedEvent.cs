namespace Shared.Events.Saga
{
    public class UserProfileUpdateFailedEvent : BaseEvent
    {
        public Guid UserId { get; set; }     // The ID of the user whose profile update failed
        public string Reason { get; set; }  // The reason for the failure
    }
}