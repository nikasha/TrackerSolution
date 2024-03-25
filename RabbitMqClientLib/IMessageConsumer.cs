public interface IMessageConsumer
{
    Task StartConsumingAsync(Action<string> onMessageReceived, CancellationToken cancellationToken);
}
