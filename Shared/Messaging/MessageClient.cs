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

        public void Listen<T>(string topic, IMessageHandler<T> handler)
        {
            _bus.PubSub.Subscribe<T>(topic, message =>
            {
                handler.Handle(message);
            });
        }
    }
}