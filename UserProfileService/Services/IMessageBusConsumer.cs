namespace UserProfileService.Services
{
    public interface IMessageBusConsumer
    {
        void StartConsuming();
        void Dispose();
    }
}