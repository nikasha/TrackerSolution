using Microsoft.EntityFrameworkCore;
using RabbitMqClientLib;
using StorageService.Db;
using StorageService.Messaging;

namespace StorageService
{
    public class Program
    {
        public static void Main(string[] args)
        {            
            var app = CreateHostBuilder(args).Build();
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }
            app.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var connectionString = hostContext.Configuration.GetConnectionString("DbConnection")?
                        .Replace("${SQL_USER}", Environment.GetEnvironmentVariable("SQL_USER"))
                        .Replace("${SQL_PASSWORD}", Environment.GetEnvironmentVariable("SQL_PASSWORD"))
                        .Replace("${SQL_DATABASE}", Environment.GetEnvironmentVariable("SQL_DATABASE"));
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(connectionString));
                    services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
                    services.AddSingleton<IRabbitConnectionFactory, RabbitConnectionFactory>();
                    services.AddHostedService<MessageConsumerBackgroundService>();
                    services.AddSingleton<IVisitService, VisitService>();
                    services.AddSingleton<IVisitMessageProcessor, VisitMessageProcessor>();
                });
    }
}