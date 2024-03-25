using RabbitMqClientLib;
using System.Text.Json;

namespace StorageService
{
    public class Worker : BackgroundService
    {
        private const string defaultFilePath = "/tmp/visits.log";
        private const string defaultFileDirectory = "/tmp";
        private readonly IMessageConsumer _messageConsumer;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Worker> _logger;
        private readonly string _logFilePath;
        private readonly Action<string> onMessageReceived;
        private readonly IMessageSender _messageSender;

        public Worker(IMessageConsumer messageConsumer, IMessageSender messageSender, IConfiguration configuration, ILogger<Worker> logger)
        {
            _messageConsumer = messageConsumer;
            _messageSender = messageSender;
            _configuration = configuration;
            _logger = logger;

            _logger.LogInformation("configuring log file for visits");

            string pathToFile = _configuration.GetValue<string>("LogFile:Path") ?? defaultFilePath;
            _logFilePath = Path.Combine(Directory.GetCurrentDirectory(), pathToFile);

            string logDirectory = Path.GetDirectoryName(_logFilePath) ?? 
                Path.Combine(Directory.GetCurrentDirectory(), defaultFileDirectory);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            _logger.LogInformation("config: {LogFilePath}", _logFilePath);

            onMessageReceived = new Action<string>(message =>
            {
                _logger.LogInformation("Message Received: {Message}", message);
                LogVisit(message);
            });
        }

        protected override async Task<bool> ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Worker starting at: {Time}", DateTimeOffset.Now);
                await _messageConsumer.StartConsumingAsync(onMessageReceived, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while starting the message consumer.");
                return false;
            }

            return true;
        }

        private void LogVisit(string message)
        {
            try
            {
                var visit = JsonSerializer.Deserialize<InfoVisitMessage>(message);
                if (visit is null)
                {
                    _logger.LogError("Error deserializing received message");
                    _messageSender.SendToDlq(message);
                }
                else
                {
                    string visitRecord = $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffffffZ}|{visit.Referer ?? "null"}|{visit.UserAgent ?? "null"}|{visit.IP}";

                    File.AppendAllText(_logFilePath, visitRecord + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging received message to the visits file");
                _messageSender.SendToDlq(message);
            }
        }
    }
}