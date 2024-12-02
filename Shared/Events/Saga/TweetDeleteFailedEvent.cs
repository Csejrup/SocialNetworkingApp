namespace Shared.Events.Saga
{
    public class TweetDeleteFailedEvent : BaseEvent
    {
        public Guid TweetId { get; set; }    // The ID of the tweet that failed to delete
        public Guid UserId { get; set; }     // The ID of the user associated with the tweet
        public string Reason { get; set; }   // The reason for the failure
    }
}