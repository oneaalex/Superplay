using Serilog;
using GameServer;
using GameServer.Handlers;

class Program
{
    static async Task Main(string[] args)
    {
        // Set up Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Debug()
            .CreateLogger();

        // Create shared context for the server (player state, etc.)
        var gameServerContext = new GameServerContext();

        // Register your message handlers
        var handlers = new List<IMessageHandler>
        {
            new LoginHandler(),
            new UpdateResourcesHandler(),
            new SendGiftHandler(),
        };
        var registry = new HandlerRegistry(handlers);

        // Create and start the server
        var server = new WebSocketGameServer(8080, registry, gameServerContext);
        await server.StartAsync();
    }
}