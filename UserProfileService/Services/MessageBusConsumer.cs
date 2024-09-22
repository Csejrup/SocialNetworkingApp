using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using UserProfileService.Models;
using System;

namespace UserProfileService.Services
{
    public class MessageBusConsumer : IMessageBusConsumer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const int RetryCount = 5;

        public MessageBusConsumer(IConnection connection, IModel channel)
        {
            _connection = connection;
            _channel = channel;
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };

            // Try connecting with retry logic
            for (int i = 0; i < RetryCount; i++)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    // Declare the exchange for tweets
                    _channel.ExchangeDeclare(exchange: "tweet_events", type: ExchangeType.Fanout);

                    // Declare a queue for UserProfileService and bind it to the exchange
                    var queueName = _channel.QueueDeclare().QueueName;
                    _channel.QueueBind(queue: queueName, exchange: "tweet_events", routingKey: "");

                    // Create a consumer to handle incoming messages
                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += (model, ea) =>
                    {
                        try
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            var tweetEvent = JsonSerializer.Deserialize<TweetEvent>(message);

                            if (tweetEvent != null)
                            {
                                HandleTweetEvent(tweetEvent);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log the error and continue processing other messages
                            Console.WriteLine($"[Error] Failed to process message: {ex.Message}");
                        }
                    };

                    // Start consuming messages
                    _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                    // Exit the retry loop as connection was successful
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Failed to connect to RabbitMQ: {ex.Message}. Retrying in 2 seconds...");
                    if (i == RetryCount - 1)
                    {
                        // Throw the exception after max retries
                        throw;
                    }
                    System.Threading.Thread.Sleep(2000); // Wait for 2 seconds before retrying
                }
            }
        }

        // Method to handle the different types of tweet events
        private void HandleTweetEvent(TweetEvent tweetEvent)
        {
            try
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
            catch (Exception ex)
            {
                // Handle any processing exceptions
                Console.WriteLine($"[Error] Error handling tweet event: {ex.Message}");
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
            try
            {
                _channel?.Close();
                _connection?.Close();
            }
            catch (Exception ex)
            {
                // Log any cleanup exceptions
                Console.WriteLine($"[Error] Error during RabbitMQ connection disposal: {ex.Message}");
            }
        }
    }
}
