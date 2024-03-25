using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMqClientLib
{
    public class RabbitMqClient : IDisposable
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private readonly ConnectionFactory _factory;

        public RabbitMqClient(ILogger<RabbitMqClient> logger)
        {
            _logger = logger;
            _factory = new ConnectionFactory() { HostName = "rabbitmq" };
            InitializeRabbitMq();
        }

        private void InitializeRabbitMq()
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "dlx.exchange.visitLog", type: "direct");

            var arguments = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "dlx.exchange.visitLog" }
            };

            _channel.QueueDeclare(queue: "visitsLogQueue",
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments);

            _channel.QueueDeclare(queue: "DLQ",
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            _channel.QueueBind(queue: "DLQ", exchange: "dlx.exchange.visitLog", routingKey: "");

            _logger.LogInformation("Queues, exchange, and bindings declared, and connection established.");
        }

        public void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "",
                                  routingKey: "visitsLogQueue",
                                  basicProperties: null,
                                  body: body);
            _logger.LogInformation("Message sent to visitsLogQueue");
        }

        public void SendToDlq(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "dlx.exchange.visitLog",
                                  routingKey: "DLQ",
                                  basicProperties: null,
                                  body: body);
            _logger.LogInformation("Message sent to DLQ");
        }

        public void StartConsuming(Action<string> onMessageReceived, CancellationToken cancellationToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                onMessageReceived(message);
                _logger.LogInformation("Message received: {Message}", message);
            };

            _channel.BasicConsume(queue: "visitsLogQueue",
                                  autoAck: true,
                                  consumer: consumer);

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
    }
}
