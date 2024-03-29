using Moq;
using RabbitMqClientLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace TestRabbitMqClientLib
{
    [TestFixture]
    public class RabbitMqClientLibUnitTests
    {
        private RabbitMqClient _rabbitMqClient;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ILogger<RabbitMqClient>> _mockLogger;
        private Mock<IRabbitConnectionFactory> _mockRabbitConnectionFactory;
        private Mock<IConnection> _mockConnection;
        private Mock<IModel> _mockChannel;

        [SetUp]
        public void SetUp()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<RabbitMqClient>>();
            _mockRabbitConnectionFactory = new Mock<IRabbitConnectionFactory>();
            _mockConnection = new Mock<IConnection>();
            _mockChannel = new Mock<IModel>();

            var mockRabbitMqConfigSection = new Mock<IConfigurationSection>();
            mockRabbitMqConfigSection.SetupGet(m => m["HostName"]).Returns("localhost");
            mockRabbitMqConfigSection.SetupGet(m => m["ExchangeName"]).Returns("exchangeName");
            mockRabbitMqConfigSection.SetupGet(m => m["VisitsLogQueueName"]).Returns("visitsLogQueue");
            mockRabbitMqConfigSection.SetupGet(m => m["DeadLetterQueueName"]).Returns("deadLetterQueue");
            mockRabbitMqConfigSection.SetupGet(m => m["DeadLetterRoutingKey"]).Returns("deadLetterRoutingKey");
            _mockConnection.Setup(c => c.CreateModel()).Returns(_mockChannel.Object);
            _mockConfiguration.Setup(c => c.GetSection("RabbitMQ")).Returns(mockRabbitMqConfigSection.Object);
            _mockRabbitConnectionFactory.Setup(c => c.CreateConnection()).Returns(_mockConnection.Object);

            _rabbitMqClient = new RabbitMqClient(_mockConfiguration.Object, _mockLogger.Object, _mockRabbitConnectionFactory.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _rabbitMqClient.Dispose();
            _mockLogger.Invocations.Clear();
            _mockConnection.Invocations.Clear();
            _mockConfiguration.Invocations.Clear();
        }

        [Test]
        public void SendMessage_ShouldPublishToCorrectQueue()
        {            
            string testMessage = "Test message";    

            _rabbitMqClient.SendMessage(testMessage);

            _mockChannel.Verify(channel =>
                channel.BasicPublish(
                    It.IsAny<string>(),
                    "visitsLogQueue",
                    false,
                    It.IsAny<IBasicProperties>(),
                     It.IsAny<ReadOnlyMemory<byte>>()),
                Times.Once);
            _mockLogger.Verify(logger =>
                logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Message sent to visitsLogQueue")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Additional tests for StartConsuming, SendToDlq, etc.
    }
}
