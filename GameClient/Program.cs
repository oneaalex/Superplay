using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Serilog;
using Shared.Messages;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Debug()
    .CreateLogger();

Log.Information("Starting GameClient...");

using var client = new ClientWebSocket();
await client.ConnectAsync(new Uri("ws://localhost:8080/ws/"), CancellationToken.None);
Log.Information("Connected to server");

// Send Login
var login = new LoginRequest { DeviceId = "my-device-123" };
var socketMessage = new SocketMessage
{
    Type = MessageType.Login,
    Payload = JsonSerializer.Serialize(login)
};
var json = JsonSerializer.Serialize(socketMessage);
await client.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, CancellationToken.None);

// Read Response
var buffer = new byte[1024 * 4];
var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
var responseJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
Log.Information("Server responded: {Response}", responseJson);

// Parse and display LoginResponse if needed
var responseMsg = JsonSerializer.Deserialize<SocketMessage>(responseJson);
if (responseMsg?.Type == MessageType.Login)
{
    var loginResp = JsonSerializer.Deserialize<LoginResponse>(responseMsg.Payload);
    Log.Information("PlayerId: {PlayerId}, Error: {Error}", loginResp?.PlayerId, loginResp?.Error);
}