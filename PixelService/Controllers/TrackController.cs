using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using RabbitMqClientLib;
using System.Text.Json;

namespace PixelService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrackController(ILogger<TrackController> logger, IMessageSender messageSender) : ControllerBase
    {
        private readonly ILogger<TrackController> _logger = logger;
        private readonly IMessageSender _messageSender = messageSender;

        [HttpGet]
        public IActionResult Get()
        {
            var referer = Request.Headers.Referer.ToString();
            var userAgent = Request.Headers.UserAgent.ToString();            
            var ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();

            _logger.LogInformation("Request received");
            var messageData = new InfoVisitMessage
            {
                Referer = referer,
                UserAgent = userAgent,
                IP = ip
            };

            string messageJson = JsonSerializer.Serialize(messageData);

            _messageSender.SendMessage(messageJson);
            _logger.LogInformation("Message sent to Storage service");

            // Create and return the image
            using var image = new Image<Rgba32>(1, 1);

            // Transparent color
            image[0, 0] = new Rgba32(255, 255, 255, 0);
            _logger.LogInformation("Transparent image was created");

            var ms = new MemoryStream();
            image.SaveAsGif(ms);
            ms.Position = 0;

            return File(ms.ToArray(), "image/gif");
        }
    }
}
