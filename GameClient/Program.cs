using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Serilog;
using Shared.Messages;

class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Debug()
            .CreateLogger();

        using var ws = new ClientWebSocket();
        var uri = new Uri("ws://localhost:8080/ws/");
        await ws.ConnectAsync(uri, CancellationToken.None);

        Log.Information("Connected to GameServer!");

        // Start a task to receive messages
        var receiveTask = Task.Run(async () =>
        {
            var buffer = new byte[4096];
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Log.Information("Server closed the connection.");
                    break;
                }
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Log.Information("Received: {Json}", json);
            }
        });

        // Main thread: Send initial login, then allow more input
        while (ws.State == WebSocketState.Open)
        {
            Console.WriteLine("Enter command (login, update, gift, quit):");
            var input = Console.ReadLine();
            if (input == "quit")
            {
                break;
            }
            SocketMessage? msg = null;
            switch (input)
            {
                case "login":
                    Console.Write("DeviceId: ");
                    var deviceId = Console.ReadLine() ?? "";
                    var loginReq = new LoginRequest { DeviceId = deviceId };
                    msg = new SocketMessage
                    {
                        Type = MessageType.Login,
                        Payload = JsonSerializer.Serialize(loginReq)
                    };
                    break;
                    // Add cases for "update", "gift" etc.
            }
            if (msg != null)
            {
                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
                await ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        // Optional: gracefully close connection
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        Log.Information("Client closed.");
    }
}