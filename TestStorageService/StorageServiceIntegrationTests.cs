using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using global::StorageService;
using RabbitMqClientLib;

namespace TestStorageService
{
    public class StorageServiceIntegrationTests
    {
        private IHost _host;
        private IConfiguration _configuration;
        private const string FilePath = "appsettings.test.json";

        [SetUp]
        public void Setup()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(FilePath, optional: true, reloadOnChange: true);

            _configuration = builder.Build();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
                    services.AddSingleton<IRabbitConnectionFactory, RabbitConnectionFactory>();
                    services.AddHostedService<FileWriter>();
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddConfiguration(_configuration);
                })
                .Build();
        }

        [TearDown]
        public async Task TearDown()
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        [Test]
        public async Task StorageService_Should_ProcessMessageAndLogInfo()
        {
            var rabbitMqClient = _host.Services.GetRequiredService<IRabbitMqClient>();
            var message = "{\"Referer\":\"https://example.com\",\"UserAgent\":\"TestAgent\",\"IP\":\"127.0.0.1\"}";
            var expectedLogFormat = $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffffffZ}|https://example.com|TestAgent|127.0.0.1";

            await _host.StartAsync();

            await Task.Delay(1000);

            rabbitMqClient.SendMessage(message);

            await Task.Delay(1000);

            Assert.That(File.Exists(_configuration.GetSection("LogFile")["Path"]));

            var logContent = File.ReadAllText(_configuration.GetSection("LogFile")["Path"] ?? "");

            Assert.That(logContent, Does.Match(expectedLogFormat));
        }
    }
    // Additional tests for handling errors, and special cases
}
