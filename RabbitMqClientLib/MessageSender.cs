using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace RabbitMqClientLib
{
    public class MessageSender : IMessageSender
    {
        private readonly ILogger<MessageConsumer> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageSender(ILogger<MessageConsumer> logger)
        {
            _logger = logger;
            var arguments = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "dlx.exchange.visitLog" }
            };
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "dlx.exchange.visitLog", type: "direct");

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

            _logger.LogInformation("Queues and exchange declared, and connection established.");
        }

        public void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "",
                                routingKey: "visitsLogQueue",
                                basicProperties: null,
                                body);
            _logger.LogInformation("Message sent to visitsLogQueue");
        }

        public void SendToDlq(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "dlx.exchange.visitLog",
                                 routingKey: "DLQ",
                                 basicProperties: null,
                                 body);
            _logger.LogInformation("Message sent to DLQ");
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
