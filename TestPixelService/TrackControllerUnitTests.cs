using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.Logging;
using RabbitMqClientLib;
using PixelService.Controllers;
using SixLabors.ImageSharp.PixelFormats;

namespace TestPixelService
{

    [TestFixture]
    public class TrackControllerUnitTests
    {        
        private Mock<ILogger<TrackController>> _loggerMock;
        private Mock<IRabbitMqClient> _rabbitMqClientMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<TrackController>>();
            _rabbitMqClientMock = new Mock<IRabbitMqClient>();
        }

        [Test]
        public void Get_ReturnsImage()
        {
            var headers = new HeaderDictionary
        {
            { "Referer", "http://example.com" },
            { "User-Agent", "UnitTestAgent" }
        };

            var contextMock = new Mock<HttpContext>();
            contextMock.SetupGet(x => x.Request.Headers).Returns(headers);
            contextMock.SetupGet(x => x.Request.HttpContext.Connection.RemoteIpAddress).Returns(IPAddress.Parse("127.0.0.1"));

            var controllerContext = new ControllerContext()
            {
                HttpContext = contextMock.Object
            };

            var controller = new TrackController(_loggerMock.Object, _rabbitMqClientMock.Object)
            {
                ControllerContext = controllerContext
            };

            var result = controller.Get();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<FileContentResult>());
                Assert.That((result as FileContentResult)?.ContentType.Equals("image/gif"), Is.True);
            });

            _rabbitMqClientMock.Verify(r => r.SendMessage(It.IsAny<string>()), Times.Once);
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o != null && o.ToString().Equals("Request received") == true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.AtLeastOnce);
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o != null && o.ToString().Equals("Message sent to Storage service") == true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.AtLeastOnce);
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o != null && o.ToString().Equals("Transparent image was created") == true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.AtLeastOnce);
        }

        // Additional tests for handling errors, and special cases
    }
}