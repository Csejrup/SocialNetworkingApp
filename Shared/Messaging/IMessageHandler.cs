namespace Shared.Messaging;

public interface IMessageHandler<T>
{
    void Handle(T message);
}
