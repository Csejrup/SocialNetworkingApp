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
            var factory = new ConnectionFactory { HostName = "rabbitmq" };

            for (var i = 0; i < RetryCount; i++)
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
                }
                else if (tweetEvent.EventType == "TweetDeleted")
                {
                    Console.WriteLine($"[UserProfileService] Tweet deleted: {tweetEvent.Content} by user {tweetEvent.UserId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Error handling tweet event: {ex.Message}");
            }
        }

        public void StartConsuming()
        {
            
        }

        public void Dispose()
        {
            try
            {
                _channel.Close();
                _connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Error during RabbitMQ connection disposal: {ex.Message}");
            }
        }
    }
}
