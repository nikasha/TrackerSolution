using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace RabbitMqClientLib
{
    public class RabbitConnectionFactory : IRabbitConnectionFactory
    {
        private readonly ConnectionFactory _connectionFactory;

        public RabbitConnectionFactory(IConfiguration configuration)
        {
            var rabbitMQConfig = configuration.GetSection("RabbitMQ");
            bool heartBeatParsed = int.TryParse(rabbitMQConfig["HeartbeatInSeconds"], out int heartBeatInSeconds);
            if (!heartBeatParsed)
            {
                heartBeatInSeconds = 60;
            }
            _connectionFactory = new ConnectionFactory()
            {
                HostName = rabbitMQConfig["HostName"],
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                TopologyRecoveryEnabled = true,
                RequestedHeartbeat = TimeSpan.FromSeconds(heartBeatInSeconds)
            };
        }

        public IConnection CreateConnection()
        {
            return _connectionFactory.CreateConnection();
        }
    }
}