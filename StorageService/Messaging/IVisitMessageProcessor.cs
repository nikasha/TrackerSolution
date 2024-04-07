namespace StorageService.Messaging
{
    public interface IVisitMessageProcessor
    {
        void ProcessMessage(string message);
    }
}