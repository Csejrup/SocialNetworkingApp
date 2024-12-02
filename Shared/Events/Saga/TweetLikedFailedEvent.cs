namespace Shared.Events.Saga
{
    public class TweetLikeFailedEvent : BaseEvent
    {
        public Guid UserId { get; set; }
        public Guid TweetId { get; set; }
        public string Reason { get; set; }
    }
}