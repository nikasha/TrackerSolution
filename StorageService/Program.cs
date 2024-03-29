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
                    services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
                    services.AddSingleton<IRabbitConnectionFactory, RabbitConnectionFactory>();
                    services.AddHostedService<FileWriter>(); // FileWriter will log the visit info into the file
                });
    }
}