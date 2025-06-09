using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Debug()
    .CreateLogger();

Log.Information("Starting GameServer...");

var server = new WebSocketGameServer(8080);
await server.StartAsync();