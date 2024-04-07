using RabbitMqClientLib;
using StorageService.Db;
using StorageService.Models;
using System.Text.Json;

namespace StorageService.Messaging
{
    public class VisitMessageProcessor(IVisitService visitService, ILogger<VisitMessageProcessor> logger, IRabbitMqClient rabbitMqClient) : IVisitMessageProcessor
    {
        private readonly ILogger<VisitMessageProcessor> _logger = logger;
        private readonly IRabbitMqClient _rabbitMqClient = rabbitMqClient;
        private readonly IVisitService _visitService = visitService;

        public void ProcessMessage(string message)
        {
            try
            {
                var visitInfo = JsonSerializer.Deserialize<InfoVisitMessage>(message);
                if (visitInfo == null)
                {
                    _logger.LogError("Error deserializing received message");
                    _rabbitMqClient.SendToDlq(message);
                    return;
                }

                var visit = new Visit
                {
                    Referer = visitInfo.Referer,
                    UserAgent = visitInfo.UserAgent,
                    IP = visitInfo.IP ?? ""
                };

                _visitService.SaveVisit(visit);

                _logger.LogInformation("The visit was logged");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error logging the received message");
                _rabbitMqClient.SendToDlq(message);
            }
        }
    }
}