using EventStore.ClientAPI;

namespace EventSourcing.API
{
    public static class EventStoreExtensions
    {
        public static void AddEventStoreConnection(this IServiceCollection services, IConfiguration configuration)
        {
            var connection = EventStoreConnection.Create(configuration.GetConnectionString("EventStore"));

            connection.ConnectAsync().Wait();

            services.AddSingleton(connection);

            using var logFactory = LoggerFactory.Create(builder => { 
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddConsole(); });

            var logger = logFactory.CreateLogger("Startup");

            connection.Connected += (s, e) =>
            {
                logger.LogInformation("EventStore connection established");
            };

            connection.ErrorOccurred += (sender, args) =>
            {
                logger.LogError(args.Exception.Message);
            };
        }

        
    }
}
