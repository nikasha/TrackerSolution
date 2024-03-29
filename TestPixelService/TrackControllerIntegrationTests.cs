
using Castle.Core.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PixelService.Controllers;
using RabbitMqClientLib;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text.Json;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace TestPixelService
{
    public class TrackControllerIntegrationTests
    {
        private WebApplicationFactory<TrackController> _factory;
        private HttpClient _client;
        private IRabbitMqClient _rabbitMqClient;
        private IConfiguration _configuration;
        private const string FilePath = "appsettings.test.json";

        [SetUp]
        public void Setup()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(FilePath, optional: true, reloadOnChange: true);

            _configuration = builder.Build();

            _factory = new WebApplicationFactory<TrackController>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
                        services.AddSingleton<IRabbitConnectionFactory, RabbitConnectionFactory>();
                        services.AddControllers();
                    }).ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.AddConfiguration(_configuration);
                    });
                });

            _client = _factory.CreateClient();

            _rabbitMqClient = _factory.Services.GetRequiredService<IRabbitMqClient>();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
            _rabbitMqClient.Dispose();
        }

        [Test]
        public async Task TrackController_SendsMessageAndReturnsImage()
        {
            string receivedMessage = string.Empty;
            _rabbitMqClient.StartConsuming((message) =>
            {
                receivedMessage = message;
            }, CancellationToken.None);

            var request = new HttpRequestMessage(HttpMethod.Get, "/track");
            request.Headers.Referrer = new Uri("http://example.com/");
            request.Headers.UserAgent.ParseAdd("UnitTestAgent");

            var response = await _client.SendAsync(request);


            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("image/gif"));
            });            

            await Task.Delay(1000);

            Assert.That(receivedMessage, Is.Not.Null);
            var messageData = JsonSerializer.Deserialize<InfoVisitMessage>(receivedMessage);
            Assert.Multiple(() =>
            {
                Assert.That(messageData, Is.Not.Null);
                Assert.That(messageData?.Referer, Is.EqualTo("http://example.com/"));
                Assert.That(messageData?.UserAgent, Is.EqualTo("UnitTestAgent"));
                Assert.That(messageData?.IP, Is.EqualTo(""));
            });

        }

        // Additional tests for handling errors, and special cases
    }
}
