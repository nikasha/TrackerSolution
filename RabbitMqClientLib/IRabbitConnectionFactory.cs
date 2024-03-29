using RabbitMQ.Client;

namespace RabbitMqClientLib
{
    public interface IRabbitConnectionFactory
    {
        IConnection CreateConnection();
    }
}