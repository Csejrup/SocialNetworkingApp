using System.Diagnostics;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using TweetPostingService.Models;

namespace TweetPostingService.Services
{
    public class MessageBusPublisher : IMessageBusPublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusPublisher()
        {
            var factory = new ConnectionFactory { HostName = "rabbitmq" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "tweet_events", type: ExchangeType.Fanout);
        }

        public void PublishTweetEvent(TweetEvent tweetEvent)
        {
            var message = JsonSerializer.Serialize(tweetEvent);
            var body = Encoding.UTF8.GetBytes(message);

          
                
            _channel.BasicPublish(exchange: "tweet_events", routingKey: "", basicProperties: null, body: body);
        }
        
        public void PublishTweetEventWithActivityContext(TweetEvent tweetEvent, Activity activity)
        {
            var message = JsonSerializer.Serialize(tweetEvent);
            var body = Encoding.UTF8.GetBytes(message);

            var props = _channel.CreateBasicProperties();
          
            var activityContext = activity?.Context ?? Activity.Current?.Context ?? default;
            var propagationContext = new PropagationContext(activityContext, Baggage.Current);
            var propagator = new TraceContextPropagator();
        
            propagator.Inject(propagationContext, props, (r, key, value) =>
            {
                r.Headers.Add(key,value);
            });

            
            _channel.BasicPublish(exchange: "tweet_events", routingKey: "", basicProperties: props, body: body);
        }
    }
}