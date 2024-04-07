using Moq;
using RabbitMqClientLib;
using Microsoft.Extensions.Logging;
using StorageService.Models;
using StorageService.Messaging;
using System.Text.Json;
using StorageService.Db;

namespace StorageService.Tests
{
    [TestFixture]
    public class VisitMessageProcessorTests
    {
        private Mock<IVisitService> _mockVisitService;
        private Mock<ILogger<VisitMessageProcessor>> _mockLogger;
        private Mock<IRabbitMqClient> _mockRabbitMqClient;
        private VisitMessageProcessor _processor;

        [SetUp]
        public void SetUp()
        {
            _mockVisitService = new Mock<IVisitService>();
            _mockLogger = new Mock<ILogger<VisitMessageProcessor>>();
            _mockRabbitMqClient = new Mock<IRabbitMqClient>();
            _processor = new VisitMessageProcessor(_mockVisitService.Object, _mockLogger.Object, _mockRabbitMqClient.Object);
        }

        [Test]
        public void ProcessMessage_ValidMessage_ShouldSaveVisit()
        {
            var validMessage = JsonSerializer.Serialize(new InfoVisitMessage
            {
                Referer = "https://example.com",
                UserAgent = "TestAgent",
                IP = "127.0.0.1"
            });

            _processor.ProcessMessage(validMessage);

            _mockVisitService.Verify(service => service.SaveVisit(It.IsAny<Visit>()), Times.Once);
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public void ProcessMessage_InvalidMessage_ShouldSendToDlq()
        {
            var invalidMessage = "Invalid JSON";

            _processor.ProcessMessage(invalidMessage);

            _mockRabbitMqClient.Verify(client => client.SendToDlq(invalidMessage), Times.Once);
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error logging the received message") || o.ToString().Contains("invalid start of a value")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        // Additional tests for serialization errors, database errors, etc.
    }
}
