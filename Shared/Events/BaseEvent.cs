namespace Shared.Events
{
    public abstract class BaseEvent
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}