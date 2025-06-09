using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Debug()
    .CreateLogger();

var server = new GameServer.WebSocketGameServer(8080);
await server.StartAsync();