using EasyNetQ;

namespace Shared.Messaging
{
    public class MessageClient
    {
        private readonly IBus _bus;

        public MessageClient(IBus bus)
        {
            _bus = bus;
        }

        public virtual void Send<T>(T message, string topic)
        {
            _bus.PubSub.Publish(message, topic);
        }

        public void Listen<T>(Action<T> handler, string topic)
        {
            _bus.PubSub.Subscribe(topic, handler);
        }
    }
}