using RabbitMqClientLib;

namespace StorageService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IMessageConsumer, MessageConsumer>();
                    services.AddSingleton<IMessageSender, MessageSender>();
                    services.AddHostedService<Worker>(); // Worker will log the info in the file
                });
    }
}