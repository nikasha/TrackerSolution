using RabbitMqClientLib;

namespace StorageService.Messaging
{
    public class MessageConsumerBackgroundService(IRabbitMqClient rabbitMqClient, ILogger<MessageConsumerBackgroundService> logger, IVisitMessageProcessor visitMessageProcessor) : BackgroundService
    {
        private readonly IRabbitMqClient _rabbitMqClient = rabbitMqClient;
        private readonly ILogger<MessageConsumerBackgroundService> _logger = logger;
        private readonly IVisitMessageProcessor _visitMessageProcessor = visitMessageProcessor;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MessageConsumerBackgroundService starting at: {Time}", DateTimeOffset.Now);
            _rabbitMqClient.StartConsuming(_visitMessageProcessor.ProcessMessage, stoppingToken);
            return Task.CompletedTask;
        }
    }

}
