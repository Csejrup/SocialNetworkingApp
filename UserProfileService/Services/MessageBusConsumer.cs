using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using UserProfileService.Models;
using System;

namespace UserProfileService.Services
{
    public class MessageBusConsumer : IMessageBusConsumer
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusConsumer()
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare the exchange for tweets
            _channel.ExchangeDeclare(exchange: "tweet_events", type: ExchangeType.Fanout);

            // Declare a queue for UserProfileService and bind it to the exchange
            var queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: queueName, exchange: "tweet_events", routingKey: "");

            // Create a consumer that will handle incoming messages
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var tweetEvent = JsonSerializer.Deserialize<TweetEvent>(message);

                if (tweetEvent != null)
                {
                    HandleTweetEvent(tweetEvent);
                }
            };

            // Start consuming messages
            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

        // Method to handle the different types of tweet events
        private void HandleTweetEvent(TweetEvent tweetEvent)
        {
            if (tweetEvent.EventType == "TweetPosted")
            {
                Console.WriteLine($"[UserProfileService] Tweet posted: {tweetEvent.Content} by user {tweetEvent.UserId}");
                // Perform additional logic here if necessary (e.g., track user activity)
            }
            else if (tweetEvent.EventType == "TweetDeleted")
            {
                Console.WriteLine($"[UserProfileService] Tweet deleted: {tweetEvent.Content} by user {tweetEvent.UserId}");
                // Perform additional logic here if necessary
            }
        }

        public void StartConsuming()
        {
            // No action needed here as the consumer is set up during instantiation.
            // This method could be expanded if you need to start and stop consumers dynamically.
        }

        public void Dispose()
        {
            // Clean up connections when the service is disposed
            _channel.Close();
            _connection.Close();
        }
    }
}
