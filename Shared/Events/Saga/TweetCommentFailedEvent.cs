namespace Shared.Events.Saga
{
    public class TweetCommentFailedEvent : BaseEvent
    {
        public Guid UserId { get; set; }
        public Guid TweetId { get; set; }
        public string Content { get; set; }
        public string Reason { get; set; }
    }
}