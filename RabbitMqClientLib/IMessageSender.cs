namespace RabbitMqClientLib
{
    public interface IMessageSender : IDisposable
    {
        void SendMessage(string message);
        void SendToDlq(string message);
    }
}
