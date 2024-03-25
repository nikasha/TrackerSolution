using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMqClientLib
{
    public class MessageConsumer(ILogger<MessageConsumer> logger) : IMessageConsumer, IDisposable
    {        
        private readonly ILogger<MessageConsumer> _logger = logger;
        private IConnection? _connection;
        private IModel? _channel;
        private string _consumerTag = "";

        public async Task StartConsumingAsync(Action<string> onMessageReceived, CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                _logger.LogInformation("Cancellation requested, closing channel...");
                _channel?.BasicCancel(_consumerTag);
                Dispose();
            });

            await ConnectToRabbitMqAsync(cancellationToken);

            var consumer = new EventingBasicConsumer(_channel);           
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                onMessageReceived(message);
                _logger.LogInformation("Message received: {Message}", message);
            };
            _consumerTag = _channel.BasicConsume(queue: "visitsLogQueue",
                                                 autoAck: true,
                                                 consumer: consumer);

            _logger.LogInformation("Started consuming messages from {QueueName}", "visitsLogQueue");
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
                _logger.LogInformation("RabbitMQ connection and channel disposed.");
            }
        }

        private async Task<bool> ConnectToRabbitMqAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            int retryCount = 0;
            var arguments = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "dlx.exchange.visitLog" }
            };

            while (!cancellationToken.IsCancellationRequested && retryCount < 5)
            {
                try
                {
                    _logger.LogInformation("Attempting to connect to RabbitMQ, attempt {Attempt}", retryCount + 1);
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


                    _logger.LogInformation("Queue declared and connection established.");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to connect to RabbitMQ, retrying in 10 seconds");
                    await Task.Delay(10000, cancellationToken);
                    retryCount++;
                }
            }

            return false;
        }
    }
}
