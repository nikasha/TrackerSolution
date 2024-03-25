namespace RabbitMqClientLib
{
    public class InfoVisitMessage
    {
        public string? Referer { get; set; }
        public string? UserAgent { get; set; }
        public required string IP { get; set; }
    }
}