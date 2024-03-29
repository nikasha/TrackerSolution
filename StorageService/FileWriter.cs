using RabbitMqClientLib;
using System.Text.Json;

namespace StorageService
{
    public class FileWriter : BackgroundService
    {
        private const string DefaultFilePath = "/tmp/visits.log";
        private readonly IRabbitMqClient _rabbitMqClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileWriter> _logger;
        // maximum file size in bytes (10MB)
        private readonly long _maxFileSizeInBytes = 10L * 1024L * 1024L;

        public FileWriter(IRabbitMqClient rabbitMqClient, IConfiguration configuration, ILogger<FileWriter> logger)
        {
            _rabbitMqClient = rabbitMqClient;
            _configuration = configuration;
            _logger = logger;
            _logger.LogInformation("Configuring log file for visits");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FileWriter starting at: {Time}", DateTimeOffset.Now);
            _rabbitMqClient.StartConsuming(LogVisit, stoppingToken);
            return Task.CompletedTask;
        }

        private void LogVisit(string message)
        {
            try
            {
                var visit = JsonSerializer.Deserialize<InfoVisitMessage>(message);
                if (visit is null)
                {
                    _logger.LogError("Error deserializing received message");
                    _rabbitMqClient.SendToDlq(message);
                }
                else
                {
                    string visitRecord = $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffffffZ}|{visit.Referer ?? "null"}|{visit.UserAgent ?? "null"}|{visit.IP}";
                    string logFilePath = GetNextAvailableLogFilePath();
                    string directoryPath = Path.GetDirectoryName(logFilePath) ?? "";

                    if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    File.AppendAllText(logFilePath, visitRecord + Environment.NewLine);
                    _logger.LogInformation("The visit was logged in the file");
                }
            }
            catch (Exception e)
            {
                if (e is JsonException || e is NotSupportedException)
                {
                    _logger.LogError("Error deserializing received message");
                }
                else
                {
                    _logger.LogError("Error logging received message into the visits file");
                }
                _rabbitMqClient.SendToDlq(message);
            }
        }

        private string GetNextAvailableLogFilePath()
        {
            string baseLogFilePath = _configuration["LogFile:Path"] ?? DefaultFilePath;
            string logFilePath = baseLogFilePath;
            int fileIndex = 0;

            while (File.Exists(logFilePath) && new FileInfo(logFilePath).Length >= _maxFileSizeInBytes)
            {
                fileIndex++;
                logFilePath = $"{baseLogFilePath}.{fileIndex}";
            }

            return logFilePath;
        }
    }
}
