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
            .WriteTo.File("client.log")
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
                try
                {
                    var socketMsg = JsonSerializer.Deserialize<SocketMessage>(json);
                    if (socketMsg != null)
                    {
                        switch (socketMsg.Type)
                        {
                            case MessageType.LoginResponse:
                                var loginResp = JsonSerializer.Deserialize<LoginResponse>(socketMsg.Payload);
                                if (!string.IsNullOrEmpty(loginResp?.Error))
                                    Log.Warning("Login failed: {Error}", loginResp.Error);
                                else
                                    Log.Information("Login successful! PlayerId: {PlayerId}", loginResp?.PlayerId);
                                break;
                            case MessageType.UpdateResourcesResponse:
                                var updateResp = JsonSerializer.Deserialize<UpdateResourcesResponse>(socketMsg.Payload);
                                if (!string.IsNullOrEmpty(updateResp?.Error))
                                    Log.Warning("Update failed: {Error}", updateResp.Error);
                                else
                                    Log.Information("Resource updated. PlayerId: {PlayerId}, Resource: {ResourceType}, New Balance: {NewBalance}", updateResp?.PlayerId, updateResp?.ResourceType, updateResp?.NewBalance);
                                break;
                            case MessageType.GiftEvent:
                                var giftEvent = JsonSerializer.Deserialize<GiftEvent>(socketMsg.Payload);
                                Log.Information("Gift Event - From: {From}, Resource: {Res}, Value: {Val}", giftEvent?.FromPlayerId, giftEvent?.ResourceType, giftEvent?.ResourceValue);
                                break;
                            case MessageType.Error:
                                Log.Error("Server error: {0}", socketMsg.Payload);
                                break;
                            default:
                                Log.Information("Received unknown message type: {Type} {Payload}", socketMsg.Type, socketMsg.Payload);
                                break;
                        }
                    }
                    else
                    {
                        Log.Warning("Could not parse incoming message: {0}", json);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception deserializing server message: {0}", json);
                }
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
                case "update":
                    Console.Write("PlayerId: ");
                    var updatePlayerId = Console.ReadLine() ?? "";
                    Console.Write("Resource (Coins, Rolls): ");
                    var resourceStr = Console.ReadLine() ?? "";
                    var resource = Enum.TryParse<ResourceType>(resourceStr, out var rt) ? rt : ResourceType.Coins;
                    Console.Write("Amount (+/-): ");
                    var amount = int.TryParse(Console.ReadLine(), out var a) ? a : 0;
                    var updateReq = new UpdateResourcesRequest
                    {
                        PlayerId = updatePlayerId,
                        ResourceType = resource,
                        ResourceValue = amount
                    };
                    msg = new SocketMessage
                    {
                        Type = MessageType.UpdateResources,
                        Payload = JsonSerializer.Serialize(updateReq)
                    };
                    break;
                case "gift":
                    Console.Write("Your PlayerId: ");
                    var fromId = Console.ReadLine() ?? "";
                    Console.Write("Friend PlayerId: ");
                    var toId = Console.ReadLine() ?? "";
                    Console.Write("Resource (Coins, Rolls): ");
                    var giftResourceStr = Console.ReadLine() ?? "";
                    var giftResource = Enum.TryParse<ResourceType>(giftResourceStr, out var grt) ? grt : ResourceType.Coins;
                    Console.Write("Amount: ");
                    var giftAmount = int.TryParse(Console.ReadLine(), out var ga) ? ga : 0;
                    var giftReq = new SendGiftRequest
                    {
                        PlayerId = fromId,
                        FriendPlayerId = toId,
                        ResourceType = giftResource,
                        ResourceValue = giftAmount
                    };
                    msg = new SocketMessage
                    {
                        Type = MessageType.SendGift,
                        Payload = JsonSerializer.Serialize(giftReq)
                    };
                    break;
                default:
                    Console.WriteLine("Unknown command. Try again.");
                    break;
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