namespace RabbitMqClientLib
{
    public interface IRabbitMqClient : IDisposable
    {
        void SendMessage(string message);
        void SendToDlq(string message);
        void StartConsuming(Action<string> onMessageReceived, CancellationToken cancellationToken);
    }
}
