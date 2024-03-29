using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMqClientLib;

namespace TestRabbitMqClientLib
{
    [TestFixture]
    public class RabbitMqClientIntegrationTests
    {
        private RabbitMqClient _client;
        private IConfiguration _configuration;
        private ILogger<RabbitMqClient> _logger;
        private const string FilePath = "appsettings.test.json";

        [SetUp]
        public void SetUp()
        {
            var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(FilePath, optional: false, reloadOnChange: true);

            _configuration = configurationBuilder.Build();
            _logger = new LoggerFactory().CreateLogger<RabbitMqClient>();
            _client = new RabbitMqClient(_configuration, _logger, new RabbitConnectionFactory(_configuration));
        }

        [Test]
        public async Task SendMessage_ShouldPublishMessageToQueueAsync()
        {
            string receivedMessage = "";
            _client.StartConsuming(msg =>
            {
                receivedMessage = msg;
            }, new CancellationToken());

            var message = "Test Message";
            _client.SendMessage(message);

            await Task.Delay(1000);

            Assert.That(receivedMessage, Is.EqualTo(message));
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
        }
    }

    // Additional tests for handling errors, and special cases
}