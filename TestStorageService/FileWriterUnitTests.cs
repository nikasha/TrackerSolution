using Moq;
using RabbitMqClientLib;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using StorageService;

namespace TestStorageService
{
    [TestFixture]
    public class FileWriterUnitTests
    {
        private const string FilePath = "/tmp/visits.test.log";
        private readonly FileWriter _fileWriter;
        private readonly Mock<IRabbitMqClient> _mockRabbitMqClient;
        private readonly Mock<ILogger<FileWriter>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public FileWriterUnitTests()
        {
            _mockRabbitMqClient = new Mock<IRabbitMqClient>();
            _mockLogger = new Mock<ILogger<FileWriter>>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["LogFile:Path"]).Returns(FilePath);           
            _fileWriter = new FileWriter(_mockRabbitMqClient.Object, _mockConfiguration.Object, _mockLogger.Object);
        }


        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _fileWriter.Dispose();
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }

        [TearDown]
        public void TearDown()
        {
            _mockLogger.Invocations.Clear();
            _mockRabbitMqClient.Invocations.Clear();
            _mockConfiguration.Invocations.Clear();
        }


        [Test]
        public void FileWriterProcess_CallsToLogAndRabbitMQWereDone()
        {
            var message = "{\"Referer\":\"https://example.com\",\"UserAgent\":\"TestAgent\",\"IP\":\"127.0.0.1\"}";
            var cancellationToken = new CancellationToken();

            _mockRabbitMqClient.Setup(x => x.StartConsuming(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()))
                .Callback<Action<string>, CancellationToken>((action, token) => action.Invoke(message));

            _fileWriter.StartAsync(cancellationToken).Wait();
            _fileWriter.StopAsync(cancellationToken).Wait();

            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o != null && o.ToString().Contains("FileWriter starting at:") == true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.AtLeastOnce);
            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o != null && o.ToString().Equals("The visit was logged in the file") == true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.AtLeastOnce);
            _mockRabbitMqClient.Verify( x => x.StartConsuming(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()));
        }

        [Test]
        public void LogVisit_WithValidMessage_CreatesExpectedLogFile()
        {
            string validMessage = "{\"Referer\":\"https://example.com\",\"UserAgent\":\"TestAgent\",\"IP\":\"127.0.0.1\"}";
            var expectedLogFormat = $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffffffZ}|https://example.com|TestAgent|127.0.0.1\n";
            var cancellationToken = new CancellationToken();

            _mockRabbitMqClient.Setup(x => x.StartConsuming(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()))
                .Callback<Action<string>, CancellationToken>((action, token) => action.Invoke(validMessage));


            _fileWriter.StartAsync(cancellationToken).Wait();
            _fileWriter.StopAsync(cancellationToken).Wait();

            Task.Delay(100).Wait();

            var logContent = File.ReadAllText(FilePath);

            Assert.That(logContent, Does.Match(expectedLogFormat));

            _fileWriter.StopAsync(cancellationToken).Wait();

            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }

        [Test]
        public void LogVisit_WithInvalidMessage_SendsToDLQ()
        {
            var cancellationToken = new CancellationToken();
            string invalidMessage = "This is not a valid JSON string";
            _mockRabbitMqClient.Setup(x => x.StartConsuming(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()))
                .Callback<Action<string>, CancellationToken>((action, token) => action.Invoke(invalidMessage));

            _fileWriter.StartAsync(cancellationToken).Wait();
            _fileWriter.StopAsync(cancellationToken).Wait();

            Task.Delay(100).Wait();

            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deserializing received message")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
            _mockRabbitMqClient.Verify(x => x.SendToDlq(invalidMessage), Times.Once);
        }

        [Test]
        public void LogVisit_CreatesNewFileWhenMaxSizeExceeded()
        {
            var message = "{\"Referer\":\"https://example.com\",\"UserAgent\":\"TestAgent\",\"IP\":\"127.0.0.1\"}";
            var additionalMessagesCount = 2;
            var cancellationToken = new CancellationToken();
            //Create a existing log file with the maximum allow size
            long fileSize = 10L * 1024 * 1024; // 10 MB
            using (var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
            {
                fs.SetLength(fileSize);
            }

            _mockConfiguration.Setup(c => c["LogFile:Path"]).Returns(FilePath);

            _mockRabbitMqClient.Setup(x => x.StartConsuming(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()))
                .Callback<Action<string>, CancellationToken>((action, token) =>
                {
                    for (int i = 0; i < additionalMessagesCount; i++)
                    {
                        action.Invoke(message);
                    }
                });

            _fileWriter.StartAsync(cancellationToken).Wait();
            _fileWriter.StopAsync(cancellationToken).Wait();
                        
            Assert.That(File.Exists(FilePath), Is.True, "First log file wasn't created");
            var rotatedLogFilePath = $"{FilePath}.1";
            Assert.That(File.Exists(rotatedLogFilePath), Is.True, "The second log file wasn't created");

            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }

            if (File.Exists(rotatedLogFilePath))
            {
                File.Delete(rotatedLogFilePath);
            }
        }

        // Additional tests for handling other deserialization errors, file rotation errors, etc.
    }
}