using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMqClientLib
{
    public class RabbitMqClient : IRabbitMqClient, IDisposable
    {

        private readonly ILogger _logger;
        private readonly IRabbitConnectionFactory _factory;
        private IConnection? _connection;
        private IModel? _channel;
        private string _exchangeName;
        private string _visitsLogQueueName;
        private string _deadLetterQueueName;
        private string _deadLetterRoutingKey;

        public RabbitMqClient(IConfiguration configuration, ILogger<RabbitMqClient> logger, IRabbitConnectionFactory factory)
        {
            _logger = logger;

            var rabbitMQConfig = configuration.GetSection("RabbitMQ");

            _factory = factory;

            InitializeRabbitMq(rabbitMQConfig);
        }

        private void InitializeRabbitMq(IConfigurationSection rabbitMQConfig)
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _exchangeName = rabbitMQConfig["ExchangeName"] ?? "";
            _visitsLogQueueName = rabbitMQConfig["VisitsLogQueueName"] ?? "";
            _deadLetterQueueName = rabbitMQConfig["DeadLetterQueueName"] ?? "";
            _deadLetterRoutingKey = rabbitMQConfig["DeadLetterRoutingKey"] ?? "";

            bool wasParsed = int.TryParse(rabbitMQConfig["MessageTTL"], out int messageTTL);
            if (!wasParsed)
            {
                messageTTL = 86400000;
            }

            _channel.ExchangeDeclare(exchange: _exchangeName,
                                     type: "direct",
                                     durable: true,
                                     autoDelete: false);

            var mainQueueArguments = new Dictionary<string, object>
            {
                { "x-message-ttl", messageTTL }, 
                { "x-dead-letter-exchange", _exchangeName }
            };

            _channel.QueueDeclare(queue: _visitsLogQueueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: mainQueueArguments);

            _channel.QueueDeclare(queue: _deadLetterQueueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            _channel.QueueBind(queue: _deadLetterQueueName,
                               exchange: _exchangeName,
                               routingKey: _deadLetterRoutingKey);

            _logger.LogInformation("Queues, exchange, and bindings declared, and connection established.");
        }

        public void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                                  routingKey: _visitsLogQueueName,
                                  basicProperties: GetPropertiesWithPersistence(),
                                  body);

            _logger.LogInformation("Message sent to visitsLogQueue");
        }

        public void SendToDlq(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel?.BasicPublish(exchange: _exchangeName,
                                  routingKey: _deadLetterRoutingKey,
                                  basicProperties: GetPropertiesWithPersistence(),
                                  body);
            _logger.LogInformation("Message sent to DLQ with routing key {DeadLetterRoutingKey}", _deadLetterRoutingKey);
        }


        public void StartConsuming(Action<string> onMessageReceived, CancellationToken cancellationToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Message received: {Message}", message);
                onMessageReceived(message);
            };

            _channel.BasicConsume(queue: _visitsLogQueueName,
                                  autoAck: true,
                                  consumer);

            cancellationToken.Register(() =>
            {
                _logger.LogInformation("Cancellation requested, closing connection...");
                Dispose();
            });

            _logger.LogInformation("Started consuming messages from visitsLogQueue");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _channel?.Dispose();
                _connection?.Dispose();
                _logger.LogInformation("RabbitMQ connection and channel disposed");
            }
        }

        private IBasicProperties? GetPropertiesWithPersistence()
        {
            var basicProperties = _channel?.CreateBasicProperties();

            if (basicProperties is not null)
            {
                basicProperties.Persistent = true;
            }

            return basicProperties;
        }
    }
}
